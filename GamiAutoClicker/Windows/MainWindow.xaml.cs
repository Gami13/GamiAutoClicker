using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using GamiAutoClicker.Components;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace GamiAutoClicker;

internal sealed partial class MainPage : Page, IDisposable
{
	private bool _isSynchronizingTiming = true;
	private readonly ClickEngine _engine;

	public MainPage(ClickEngine engine)
	{
		_engine = engine;
		InitializeComponent();
		RefreshLanguage();
		MouseButtonComboBox.SelectedIndex = _engine.MouseButton switch {
            VirtualKey.RightButton => 1, VirtualKey.MiddleButton => 2,
            VirtualKey.XButton1 => 3, VirtualKey.XButton2 => 4, _ => 0 };
		HoldModeToggleSwitch.IsOn = _engine.HoldMode;
		ClickingEnabledToggleSwitch.IsOn = _engine.Enabled;
		SyncTimingInputs(updateDelayBox: true, updateCpsBox: true);
		UpdateClickingStatus();
		MouseButtonComboBox.SelectionChanged += OnMouseButtonSelectionChanged;
		HoldModeToggleSwitch.Toggled += OnHoldModeToggled;
		_engine.ToggleRequested += OnToggleRequested;
        _engine.ClickingFailed += OnClickingFailed;
        GeneralSettings.Changed += ApplyGeneralSettings;

	}

    public void Dispose() {
        _isSynchronizingTiming = true;
        MouseButtonComboBox.SelectionChanged -= OnMouseButtonSelectionChanged;
        HoldModeToggleSwitch.Toggled -= OnHoldModeToggled;
        ClickingEnabledToggleSwitch.Toggled -= OnClickingEnabledToggled;
        DelayNumberBox.ValueChanged -= OnDelayValueChanged;
        CpsNumberBox.ValueChanged -= OnCpsValueChanged;
        _engine.ToggleRequested -= OnToggleRequested;
        _engine.ClickingFailed -= OnClickingFailed;
        GeneralSettings.Changed -= ApplyGeneralSettings;
    }

    private void OnToggleRequested() => ClickingEnabledToggleSwitch.IsOn = !ClickingEnabledToggleSwitch.IsOn;

	private void RefreshLanguage()
    {
        int selected = MouseButtonComboBox.SelectedIndex;
        MouseButtonComboBox.ItemsSource = new[] {
            Localization.Format("MouseTarget", 1), Localization.Format("MouseTarget", 2),
            Localization.Format("MouseTarget", 3), Localization.Format("MouseTarget", 4), Localization.Format("MouseTarget", 5) };
        MouseButtonComboBox.SelectedIndex = selected < 0 ? 0 : selected;
        UpdateClickingStatus();
        UpdateKeyHints();
    }

	private void ApplyGeneralSettings()
    {
        ClickingEnabledToggleSwitch.IsOn = false;
        _engine.Enabled = false;
        _engine.ToggleKey = GeneralSettings.ToggleKey;
        _engine.HoldKey = GeneralSettings.HoldKey;
        UpdateKeyHints();
    }

    private void UpdateKeyHints()
    {
        ToolTipService.SetToolTip(ClickingEnabledToggleSwitch, Localization.Format("ToggleHint", GeneralSettings.KeyLabel(_engine.ToggleKey)));
        ToolTipService.SetToolTip(HoldModeInfoIcon, Localization.Format("HoldHint", GeneralSettings.KeyLabel(_engine.HoldKey)));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(ClickingEnabledToggleSwitch, Localization.Get("Enableclicking"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(HoldModeToggleSwitch, Localization.Get("Holdmode"));
        Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(MouseButtonComboBox, Localization.Get("Clicktarget"));
    }

    private void OnClickingFailed(string message)
	{
		ClickingEnabledToggleSwitch.IsOn = false;
		StatusTextBlock.Text = Localization.Get("Blocked");
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
		if (_isSynchronizingTiming) return;
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
		StatusTextBlock.Text = Localization.Get(isEnabled ? "Enabled" : "Disabled");
		ToolTipService.SetToolTip(StatusTextBlock, null);

		string resourceKey = isEnabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush";
		if (Application.Current.Resources.TryGetValue(resourceKey, out object brushObject) && brushObject is Brush brush)
		{
			StatusIndicator.Fill = brush;
		}
	}

}
