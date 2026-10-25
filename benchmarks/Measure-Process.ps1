#Requires -Version 5.1
param(
    [Parameter(Mandatory = $true)][string]$Executable,
    [ValidateSet('main', 'settings')][string]$Scenario = 'main',
    [ValidateRange(1, 100)][int]$Runs = 3,
    [ValidateRange(1, 3600)][int]$DurationSeconds = 30,
    [ValidateRange(0, 300)][int]$WarmupSeconds = 10,
    [string]$OutputDirectory = (Join-Path $PSScriptRoot 'results')
)

$ErrorActionPreference = 'Stop'
if (-not ('ProcessMemoryInfo' -as [type])) {
Add-Type -TypeDefinition @'
using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct ProcessMemoryCountersEx2 {
    public uint cb, PageFaultCount;
    public UIntPtr PeakWorkingSetSize, WorkingSetSize;
    public UIntPtr QuotaPeakPagedPoolUsage, QuotaPagedPoolUsage;
    public UIntPtr QuotaPeakNonPagedPoolUsage, QuotaNonPagedPoolUsage;
    public UIntPtr PagefileUsage, PeakPagefileUsage, PrivateUsage, PrivateWorkingSetSize;
    public ulong SharedCommitUsage;
}

public static class ProcessMemoryInfo {
    [DllImport("psapi.dll", SetLastError = true)]
    public static extern bool GetProcessMemoryInfo(IntPtr process, ref ProcessMemoryCountersEx2 counters, uint size);
}
'@
}
$exe = (Resolve-Path -LiteralPath $Executable).Path
$output = [System.IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $output -Force | Out-Null
$allSamples = [System.Collections.Generic.List[object]]::new()
$summaries = [System.Collections.Generic.List[object]]::new()

for ($run = 1; $run -le $Runs; $run++) {
    $ready = Join-Path $output "ready-$run.txt"
    if (Test-Path -LiteralPath $ready) { Remove-Item -LiteralPath $ready }
    $previousScenario = [System.Environment]::GetEnvironmentVariable('GAMI_PERF_SCENARIO')
    $previousReady = [System.Environment]::GetEnvironmentVariable('GAMI_PERF_READY_FILE')
    try {
        $env:GAMI_PERF_SCENARIO = $Scenario
        $env:GAMI_PERF_READY_FILE = $ready
        $process = Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe) -WindowStyle Hidden -PassThru
    }
    finally {
        [System.Environment]::SetEnvironmentVariable('GAMI_PERF_SCENARIO', $previousScenario)
        [System.Environment]::SetEnvironmentVariable('GAMI_PERF_READY_FILE', $previousReady)
    }

    try {
        $deadline = [System.Diagnostics.Stopwatch]::StartNew()
        while ($deadline.Elapsed.TotalSeconds -lt 30) {
            $process.Refresh()
            if ($process.HasExited) { throw "Run $run exited before the window was ready (exit $($process.ExitCode))." }
            if ($process.MainWindowHandle -ne 0 -and (Test-Path -LiteralPath $ready)) { break }
            Start-Sleep -Milliseconds 250
        }
        if ($process.MainWindowHandle -eq 0 -or !(Test-Path -LiteralPath $ready)) {
            throw "Run $run did not expose a window and benchmark ready marker. Build with PERF_BENCHMARK."
        }
        if ((Get-Content -LiteralPath $ready -Raw).Trim() -ne $Scenario) {
            throw "Run $run reported the wrong benchmark scenario."
        }
        Start-Sleep -Seconds $WarmupSeconds
        $clock = [System.Diagnostics.Stopwatch]::StartNew()
        $runSamples = [System.Collections.Generic.List[object]]::new()
        for ($second = 0; $second -le $DurationSeconds; $second++) {
            $process.Refresh()
            if ($process.HasExited) { throw "Run $run exited during sampling." }
            $memory = [ProcessMemoryCountersEx2]::new()
            $memory.cb = [System.Runtime.InteropServices.Marshal]::SizeOf([type][ProcessMemoryCountersEx2])
            if (![ProcessMemoryInfo]::GetProcessMemoryInfo($process.Handle, [ref]$memory, $memory.cb)) {
                throw "GetProcessMemoryInfo failed: $([System.Runtime.InteropServices.Marshal]::GetLastWin32Error())"
            }
            $sample = [pscustomobject]@{
                Run = $run
                Scenario = $Scenario
                Utc = [System.DateTime]::UtcNow.ToString('o')
                ElapsedSeconds = $clock.Elapsed.TotalSeconds
                CpuSeconds = $process.TotalProcessorTime.TotalSeconds
                PrivateBytes = $process.PrivateMemorySize64
                WorkingSetBytes = $process.WorkingSet64
                PrivateWorkingSetBytes = $memory.PrivateWorkingSetSize.ToUInt64()
                Handles = $process.HandleCount
                Threads = $process.Threads.Count
            }
            $runSamples.Add($sample)
            $allSamples.Add($sample)
            if ($second -lt $DurationSeconds) { Start-Sleep -Seconds 1 }
        }
        $first = $runSamples[0]
        $last = $runSamples[$runSamples.Count - 1]
        $wall = $last.ElapsedSeconds - $first.ElapsedSeconds
        $cpu = $last.CpuSeconds - $first.CpuSeconds
        $private = @($runSamples | ForEach-Object PrivateBytes | Sort-Object)
        $working = @($runSamples | ForEach-Object WorkingSetBytes | Sort-Object)
        $privateWorking = @($runSamples | ForEach-Object PrivateWorkingSetBytes | Sort-Object)
        $middle = [int][math]::Floor($runSamples.Count / 2)
        $summary = [pscustomobject]@{
            Run = $run
            Scenario = $Scenario
            CpuSeconds = [math]::Round($cpu, 3)
            OneCorePercent = [math]::Round(100 * $cpu / $wall, 2)
            PrivateMedianMB = [math]::Round($private[$middle] / 1MB, 1)
            PrivatePeakMB = [math]::Round($private[-1] / 1MB, 1)
            WorkingMedianMB = [math]::Round($working[$middle] / 1MB, 1)
            WorkingPeakMB = [math]::Round($working[-1] / 1MB, 1)
            TaskManagerMemoryMedianMB = [math]::Round($privateWorking[$middle] / 1MB, 1)
            TaskManagerMemoryPeakMB = [math]::Round($privateWorking[-1] / 1MB, 1)
        }
        $summaries.Add($summary)
        $summary | ConvertTo-Json -Compress | Write-Host
    }
    finally {
        $process.Refresh()
        if (!$process.HasExited) { Stop-Process -Id $process.Id -Force }
        $process.Dispose()
        if (Test-Path -LiteralPath $ready) { Remove-Item -LiteralPath $ready }
    }
}

$allSamples | Export-Csv -LiteralPath (Join-Path $output "$Scenario-samples.csv") -NoTypeInformation
$summaries | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $output "$Scenario-summary.json")
