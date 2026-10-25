# Longer Reactor comparison — 2026-10-25

This repeats the idle comparison with 20 seconds of warmup followed by 90 seconds of sampling at roughly one sample per second. Each scenario/build has two fresh-process runs, ordered original–Reactor–Reactor–original for the main window and then the settings window. The benchmark starts hidden windows with clicking disabled and waits for each window's ready marker. Both measured executables are local self-contained managed .NET 10 Release builds; their loaded `coreclr.dll`, `Microsoft.UI.Xaml.dll`, and Windows App Runtime bootstrap files match. The original executable is `GamiAutoClicker/bin/perf/GamiAutoClicker.exe` and the port is `GamiAutoClicker.Reactor/bin/x64/Release/net10.0-windows10.0.26100.0/win-x64/GamiAutoClicker.Reactor.exe`.

Task Manager Memory below is **private working set**, the resident memory attributed to this process. Private commit and total working set answer different questions. CPU is process CPU time divided by elapsed time as a percentage of **one logical core**, not the whole machine. Values in the table are the average of the two run medians for memory, and of their measured 90-second CPU rates.

| Scenario | Build | Task Manager memory | Private commit | Total working set | Idle CPU, one core | CPU in final 30 seconds, one core |
| --- | --- | ---: | ---: | ---: | ---: | ---: |
| Main | Original | 43.80 MB | 105.25 MB | 158.10 MB | 2.52% | 2.92% |
| Main | Reactor | 41.85 MB | 104.50 MB | 163.35 MB | 4.36% | 4.86% |
| Settings open | Original | 55.30 MB | 120.55 MB | 179.90 MB | 2.41% | 2.82% |
| Settings open | Reactor | 48.60 MB | 111.70 MB | 176.45 MB | 3.76% | 3.86% |

The Reactor window uses **1.95 MB less** Task Manager memory on main, and **6.70 MB less** with settings open. On main, its *total* working set is 5.25 MB higher; total working set includes shared resident pages, so it should not be substituted for Task Manager Memory. Reactor's measured CPU rate is higher by 1.84 percentage points of one core on main and 1.35 with settings open. That difference persists when only the final 30 seconds are considered. CPU varied between individual runs, particularly Reactor main (3.89% and 4.82% for the full measurement; 3.36% and 6.36% in the final 30 seconds), so these results indicate higher idle CPU in these runs but do not identify its cause.

All eight windows gained roughly 4–5 MB of private working set between the first and last samples. The 90-second memory medians therefore describe the measurement window, not a proven long-term plateau. Active clicking, visible rendering, interactions, and battery use were not measured. As with the earlier comparison, these executable builds precede the port's final translation embedding and backdrop/theme changes; the current environment lacks the .NET 10 SDK needed to rebuild both consistently. The measured result applies to the binaries listed above.

## Individual runs

| Scenario | Build | Run | Task Manager memory | Private commit | Total working set | CPU, one core | Final 30-second CPU |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Main | Original | 1 | 43.8 MB | 105.6 MB | 158.0 MB | 2.46% | 2.94% |
| Main | Original | 2 | 43.8 MB | 104.9 MB | 158.2 MB | 2.58% | 2.89% |
| Main | Reactor | 1 | 41.8 MB | 103.7 MB | 163.3 MB | 3.89% | 3.36% |
| Main | Reactor | 2 | 41.9 MB | 105.3 MB | 163.4 MB | 4.82% | 6.36% |
| Settings | Original | 1 | 55.4 MB | 121.6 MB | 180.1 MB | 2.57% | 2.79% |
| Settings | Original | 2 | 55.2 MB | 119.5 MB | 179.7 MB | 2.24% | 2.84% |
| Settings | Reactor | 1 | 48.5 MB | 111.9 MB | 176.2 MB | 3.50% | 3.99% |
| Settings | Reactor | 2 | 48.7 MB | 111.5 MB | 176.7 MB | 4.02% | 3.72% |

The [benchmark script](Measure-Process.ps1) produced per-second CSV data and JSON run summaries under `benchmarks/results/long-{scenario}-{build}-{run}/`. Those raw result folders are ignored by Git. See [the earlier short comparison](reactor-2026-10-25.md) for context.
