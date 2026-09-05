using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using GamiAutoClicker.Components;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker;

public sealed partial class MainWindow : Window
{
	private const double InitialDelayMilliseconds = 100d;
	private bool _isSynchronizingTiming;
	private double _delayMilliseconds = InitialDelayMilliseconds;

	public MainWindow()
	{
		InitializeComponent();
		SyncTimingInputs(updateDelayBox: true, updateCpsBox: true);
		UpdateClickingStatus();
		Closed += MainWindow_Closed;
	}

	private void OnDelayValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isSynchronizingTiming || double.IsNaN(args.NewValue)) return;

		double delay = Math.Clamp(args.NewValue, DelayNumberBox.Minimum, DelayNumberBox.Maximum);
		if (!double.IsFinite(delay) || delay <= 0) return;

		_delayMilliseconds = delay;

		SyncTimingInputs(updateDelayBox: false, updateCpsBox: true);
	}

	private void OnCpsValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isSynchronizingTiming || double.IsNaN(args.NewValue)) return;

		double cps = Math.Clamp(args.NewValue, CpsNumberBox.Minimum, CpsNumberBox.Maximum);
		if (!double.IsFinite(cps) || cps <= 0) return;

		_delayMilliseconds = 1000d / cps;

		SyncTimingInputs(updateDelayBox: true, updateCpsBox: false);
	}

	private void OnClickingEnabledToggled(object sender, RoutedEventArgs e)
	{
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

		string resourceKey = isEnabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush";
		if (Application.Current.Resources.TryGetValue(resourceKey, out object brushObject) && brushObject is Brush brush)
		{
			StatusIndicator.Fill = brush;
		}
	}

	private void MainWindow_Closed(object? sender, WindowEventArgs args)
	{
		Application.Current.Exit();
	}
}

