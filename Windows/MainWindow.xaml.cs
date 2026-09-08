using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using GamiAutoClicker.Components;
using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace GamiAutoClicker;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "The window's Closed handler cancels, awaits and disposes the clicking lifetime.")]
public sealed partial class MainWindow : Window
{
	private const double InitialDelayMilliseconds = 100d;
	private bool _isSynchronizingTiming;
	private double _delayMilliseconds = InitialDelayMilliseconds;
	private readonly ClickEngine _engine = new();
	private readonly CancellationTokenSource _clickingCancellation = new();
	private readonly Task _clickingTask;

	public MainWindow()
	{
		InitializeComponent();
		SyncTimingInputs(updateDelayBox: true, updateCpsBox: true);
		UpdateClickingStatus();
		Closed += MainWindow_Closed;
		MouseButtonComboBox.SelectionChanged += OnMouseButtonSelectionChanged;
		HoldModeToggleSwitch.Toggled += OnHoldModeToggled;
		_engine.ToggleRequested += () => ClickingEnabledToggleSwitch.IsOn = !ClickingEnabledToggleSwitch.IsOn;
		ToolTipService.SetToolTip(ClickingEnabledToggleSwitch, $"{_engine.ToggleKey}: enable or disable clicking globally");
		ToolTipService.SetToolTip(HoldModeInfoIcon, "While clicking is enabled, hold Mouse 4 (side/back button) to click. Release it to stop. Choose a different button as the click target.");
		_clickingTask = RunClickingAsync();
	}

	private async Task RunClickingAsync()
	{
		while (!_clickingCancellation.IsCancellationRequested)
		{
			try
			{
				await _engine.RunAsync(_clickingCancellation.Token).ConfigureAwait(true);
			}
			catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
			{
				ClickingEnabledToggleSwitch.IsOn = false;
				StatusTextBlock.Text = "Blocked";
				ToolTipService.SetToolTip(StatusTextBlock, exception.Message);
			}
		}
	}

	private void OnMouseButtonSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_engine.MouseButton = MouseButtonComboBox.SelectedIndex switch
		{
			0 => VirtualKey.LeftButton,
			1 => VirtualKey.RightButton,
			2 => VirtualKey.MiddleButton,
			3 => VirtualKey.XButton1,
			4 => VirtualKey.XButton2,
			_ => VirtualKey.LeftButton
		};
	}

	private void OnHoldModeToggled(object sender, RoutedEventArgs e)
	{
		_engine.HoldMode = HoldModeToggleSwitch.IsOn;
	}

	private void OnDelayValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isSynchronizingTiming || double.IsNaN(args.NewValue)) return;

		double delay = Math.Clamp(args.NewValue, DelayNumberBox.Minimum, DelayNumberBox.Maximum);
		if (!double.IsFinite(delay) || delay <= 0) return;

		_delayMilliseconds = delay;
		_engine.IntervalMilliseconds = delay;

		SyncTimingInputs(updateDelayBox: false, updateCpsBox: true);
	}

	private void OnCpsValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isSynchronizingTiming || double.IsNaN(args.NewValue)) return;

		double cps = Math.Clamp(args.NewValue, CpsNumberBox.Minimum, CpsNumberBox.Maximum);
		if (!double.IsFinite(cps) || cps <= 0) return;

		_delayMilliseconds = 1000d / cps;
		_engine.IntervalMilliseconds = _delayMilliseconds;

		SyncTimingInputs(updateDelayBox: true, updateCpsBox: false);
	}

	private void OnClickingEnabledToggled(object sender, RoutedEventArgs e)
	{
		_engine.Enabled = ClickingEnabledToggleSwitch.IsOn;
		UpdateClickingStatus();
	}

	private void SyncTimingInputs(bool updateDelayBox, bool updateCpsBox)
	{
		_isSynchronizingTiming = true;
		try
		{
			if (updateDelayBox)
			{
				DelayNumberBox.Value = _delayMilliseconds;
			}

			if (updateCpsBox)
			{
				// UnitNumberBox rounds only its display; retain the exact reciprocal here.
				CpsNumberBox.Value = 1000d / _delayMilliseconds;
			}
		}
		finally
		{
			_isSynchronizingTiming = false;
		}
	}

	private void UpdateClickingStatus()
	{
		bool isEnabled = ClickingEnabledToggleSwitch.IsOn;
		StatusTextBlock.Text = isEnabled ? "Enabled" : "Disabled";
		ToolTipService.SetToolTip(StatusTextBlock, null);

		string resourceKey = isEnabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush";
		if (Application.Current.Resources.TryGetValue(resourceKey, out object brushObject) && brushObject is Brush brush)
		{
			StatusIndicator.Fill = brush;
		}
	}

	private async void MainWindow_Closed(object? sender, WindowEventArgs args)
	{
		_clickingCancellation.Cancel();
		await _clickingTask.ConfigureAwait(true);
		_clickingCancellation.Dispose();
		Application.Current.Exit();
	}
}

