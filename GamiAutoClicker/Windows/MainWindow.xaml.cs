using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using GamiAutoClicker.Components;
using Microsoft.UI.Xaml.Controls;
using System.Threading;
using System.Threading.Tasks;
using Windows.System;

namespace GamiAutoClicker;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "The window's Closed handler cancels, awaits and disposes the clicking lifetime.")]
public sealed partial class MainWindow : Window
{
	private bool _isSynchronizingTiming;
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
		_engine.ClickingFailed += OnClickingFailed;
		GeneralSettings.Changed += ApplyGeneralSettings;
        ApplyGeneralSettings();
		_clickingTask = _engine.RunAsync(_clickingCancellation.Token);
	}

	private void ApplyGeneralSettings()
    {
        ClickingEnabledToggleSwitch.IsOn = false;
        _engine.ToggleKey = GeneralSettings.ToggleKey;
        _engine.HoldKey = GeneralSettings.HoldKey;
        ToolTipService.SetToolTip(ClickingEnabledToggleSwitch, $"{GeneralSettings.KeyLabel(_engine.ToggleKey)}: enable or disable clicking globally");
        ToolTipService.SetToolTip(HoldModeInfoIcon, $"While clicking is enabled, hold {GeneralSettings.KeyLabel(_engine.HoldKey)} to click. Release it to stop. Choose a different button as the click target.");
    }

    private void OnClickingFailed(string message)
	{
		ClickingEnabledToggleSwitch.IsOn = false;
		StatusTextBlock.Text = "Blocked";
		ToolTipService.SetToolTip(StatusTextBlock, message);
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

		_engine.IntervalMilliseconds = delay;

		SyncTimingInputs(updateDelayBox: false, updateCpsBox: true);
	}

	private void OnCpsValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isSynchronizingTiming || double.IsNaN(args.NewValue)) return;

		double cps = Math.Clamp(args.NewValue, CpsNumberBox.Minimum, CpsNumberBox.Maximum);
		if (!double.IsFinite(cps) || cps <= 0) return;

		_engine.IntervalMilliseconds = 1000d / cps;

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
				DelayNumberBox.Value = _engine.IntervalMilliseconds;
			}

			if (updateCpsBox)
			{
				// UnitNumberBox rounds only its display; retain the exact reciprocal here.
				CpsNumberBox.Value = 1000d / _engine.IntervalMilliseconds;
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
		GeneralSettings.Changed -= ApplyGeneralSettings;
        GeneralSettings.IsEditing = false;
        _clickingCancellation.Cancel();
		await _clickingTask.ConfigureAwait(true);
		_clickingCancellation.Dispose();
		Application.Current.Exit();
	}
}
