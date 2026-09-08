using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace GamiAutoClicker;

// Used on the UI thread: awaiting the delay keeps the window responsive without shared state or locks.
internal sealed class ClickEngine
{
	internal const double MinimumIntervalMilliseconds = 15;
	internal const double MaximumClicksPerSecond = 1000d / MinimumIntervalMilliseconds;
	public double IntervalMilliseconds { get; set; } = 100;
	public double RandomOffsetMilliseconds { get; set; }
	public bool Enabled { get; set; }
	public bool HoldMode { get; set; }
	public VirtualKey ToggleKey { get; set; } = VirtualKey.F8;
	public VirtualKey HoldKey { get; set; } = VirtualKey.XButton1;
	public VirtualKey MouseButton { get; set; } = VirtualKey.LeftButton;

	public event Action? ToggleRequested;
	public event Action<string>? ClickingFailed;

	public async Task RunAsync(CancellationToken cancellationToken)
	{
		bool toggleWasDown = IsKeyDown(ToggleKey);
		bool wasActive = false;
        bool wasEditing = GeneralSettings.IsEditing;
		long lastClick = 0;
		double nextInterval = IntervalMilliseconds;

		try
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				bool toggleDown = IsKeyDown(ToggleKey);
				if (toggleDown && !toggleWasDown && !GeneralSettings.IsEditing && !wasEditing)
				{
					ToggleRequested?.Invoke();
				}
				toggleWasDown = toggleDown;
                wasEditing = GeneralSettings.IsEditing;

				// A trigger must differ from the output; injected releases would otherwise affect its state.
				if (Enabled && (ToggleKey == MouseButton || (HoldMode && HoldKey == MouseButton)))
				{
					DisableClicking(Localization.Get("TheactivekeybindsandclicktargetmustbedifferentChooseanotherkeybindorclicktarget"));
				}
				bool active = Enabled && !GeneralSettings.IsEditing && (!HoldMode || IsKeyDown(HoldKey));
				if (active && (!wasActive || Stopwatch.GetElapsedTime(lastClick).TotalMilliseconds >= nextInterval))
				{
					try
					{
						Click();
						lastClick = Stopwatch.GetTimestamp();
						nextInterval = GetNextIntervalMilliseconds();
					}
					catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
					{
						DisableClicking(exception.Message);
						active = false;
					}
				}
				wasActive = active;

				// Check keys frequently even when the interval is several minutes long.
				double delay = active
					? Math.Clamp(nextInterval - Stopwatch.GetElapsedTime(lastClick).TotalMilliseconds, 1, 10)
					: 10;
				await Task.Delay(TimeSpan.FromMilliseconds(delay), cancellationToken).ConfigureAwait(true);
			}
		}
		catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
		{
			// Closing the window cancels the pending delay.
		}
		finally
		{
			Enabled = false;
		}
	}

	// Sample once per click, not once per keyboard poll. Offset is the full range width.
	internal double GetNextIntervalMilliseconds() => Math.Max(MinimumIntervalMilliseconds,
		IntervalMilliseconds + (Random.Shared.NextDouble() - 0.5) * RandomOffsetMilliseconds);

	private void DisableClicking(string message)
	{
		Enabled = false;
		ClickingFailed?.Invoke(message);
	}

	private void Click()
	{
		(uint down, uint up, uint data) = MouseButton switch
		{
			VirtualKey.LeftButton => (0x0002u, 0x0004u, 0u),
			VirtualKey.RightButton => (0x0008u, 0x0010u, 0u),
			VirtualKey.MiddleButton => (0x0020u, 0x0040u, 0u),
			VirtualKey.XButton1 => (0x0080u, 0x0100u, 1u),
			VirtualKey.XButton2 => (0x0080u, 0x0100u, 2u),
			_ => throw new InvalidOperationException(Localization.Get("Chooseamousebuttonastheclicktarget"))
		};
		Input[] inputs =
		[
			new() { Mouse = new() { Flags = down, MouseData = data } },
			new() { Mouse = new() { Flags = up, MouseData = data } }
		];
		uint sent = SendInput(2, inputs, Marshal.SizeOf<Input>());
		if (sent != 2)
		{
			int error = Marshal.GetLastPInvokeError();
			if (sent == 1)
			{
				// Attempt the release if Windows accepted only the press.
				_ = SendInput(1, [inputs[1]], Marshal.SizeOf<Input>());
			}
			throw new Win32Exception(error, Localization.Get("WindowsblockedtheclickClickinghasbeendisabled"));
		}
	}

	private static bool IsKeyDown(VirtualKey key) => key != VirtualKey.None && GetAsyncKeyState((int)key) < 0;

	// MOUSEINPUT is the largest member of the native INPUT union, so it supplies its size and alignment.
	[StructLayout(LayoutKind.Sequential)]
	private struct Input
	{
		public uint Type; // INPUT_MOUSE = 0
		public MouseInput Mouse;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct MouseInput
	{
		public int X, Y;
		public uint MouseData, Flags, Time;
		public nuint ExtraInfo;
	}

	[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern uint SendInput(uint count, [In] Input[] inputs, int size);

	[DllImport("user32.dll", ExactSpelling = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	private static extern short GetAsyncKeyState(int key);
}
