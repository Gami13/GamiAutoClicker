using GamiAutoClicker.Components;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Linq;
using Windows.System;

namespace GamiAutoClicker;

internal sealed partial class MainPage : Page, IDisposable {
	private static readonly VirtualKey[] MouseButtons = [
		VirtualKey.LeftButton, VirtualKey.RightButton, VirtualKey.MiddleButton,
		VirtualKey.XButton1, VirtualKey.XButton2
	];
	private readonly ClickEngine _engine;
	private bool _isSynchronizing = true;

	public MainPage(ClickEngine engine) {
		_engine = engine;
		InitializeComponent();
		InitializeControls();
		MouseButtonComboBox.SelectionChanged += OnMouseButtonSelectionChanged;
		HoldModeToggleSwitch.Toggled += OnHoldModeToggled;
		_engine.ToggleRequested += OnToggleRequested;
		_engine.ClickingFailed += OnClickingFailed;
		GeneralSettings.Changed += ApplyGeneralSettings;
	}

	public void Dispose() {
		_isSynchronizing = true;
		MouseButtonComboBox.SelectionChanged -= OnMouseButtonSelectionChanged;
		HoldModeToggleSwitch.Toggled -= OnHoldModeToggled;
		ClickingEnabledToggleSwitch.Toggled -= OnClickingEnabledToggled;
		DelayNumberBox.ValueChanged -= OnTimingValueChanged;
		CpsNumberBox.ValueChanged -= OnTimingValueChanged;
		_engine.ToggleRequested -= OnToggleRequested;
		_engine.ClickingFailed -= OnClickingFailed;
		GeneralSettings.Changed -= ApplyGeneralSettings;
	}

	private void InitializeControls() {
		MouseButtonComboBox.ItemsSource = Enumerable.Range(1, MouseButtons.Length)
			.Select(button => Localization.Format("MouseTarget", button)).ToArray();
		MouseButtonComboBox.SelectedIndex = Math.Max(0, Array.IndexOf(MouseButtons, _engine.MouseButton));
		HoldModeToggleSwitch.IsOn = _engine.HoldMode;
		ClickingEnabledToggleSwitch.IsOn = _engine.Enabled;
		SyncTimingInputs();
		UpdateClickingStatus();
		UpdateKeyHints();
	}

	private void ApplyGeneralSettings() {
		ClickingEnabledToggleSwitch.IsOn = false;
		_engine.Enabled = false;
		_engine.ToggleKey = GeneralSettings.ToggleKey;
		_engine.HoldKey = GeneralSettings.HoldKey;
		UpdateKeyHints();
	}

	private void UpdateKeyHints() {
		ToolTipService.SetToolTip(ClickingEnabledToggleSwitch, Localization.Format("ToggleHint", GeneralSettings.KeyLabel(_engine.ToggleKey)));
		ToolTipService.SetToolTip(HoldModeInfoIcon, Localization.Format("HoldHint", GeneralSettings.KeyLabel(_engine.HoldKey)));
		AutomationProperties.SetName(ClickingEnabledToggleSwitch, Localization.Get("Enableclicking"));
		AutomationProperties.SetName(HoldModeToggleSwitch, Localization.Get("Holdmode"));
		AutomationProperties.SetName(MouseButtonComboBox, Localization.Get("Clicktarget"));
	}

	private void OnMouseButtonSelectionChanged(object sender, SelectionChangedEventArgs e) =>
		_engine.MouseButton = MouseButtons[Math.Max(0, MouseButtonComboBox.SelectedIndex)];

	private void OnHoldModeToggled(object sender, RoutedEventArgs e) => _engine.HoldMode = HoldModeToggleSwitch.IsOn;

	private void OnTimingValueChanged(UnitNumberBox sender, NumberBoxValueChangedEventArgs args) {
		if (_isSynchronizing || double.IsNaN(args.NewValue)) return;

		double value = Math.Clamp(args.NewValue, sender.Minimum, sender.Maximum);
		if (!double.IsFinite(value) || value <= 0) return;

		_engine.IntervalMilliseconds = ReferenceEquals(sender, DelayNumberBox) ? value : 1000d / value;
		SyncTimingInputs(editedBox: sender);
	}

	private void SyncTimingInputs(UnitNumberBox? editedBox = null) {
		_isSynchronizing = true;
		try {
			// Leave the edited box alone; UnitNumberBox rounds display values, not engine timing.
			if (!ReferenceEquals(editedBox, DelayNumberBox)) DelayNumberBox.Value = _engine.IntervalMilliseconds;
			if (!ReferenceEquals(editedBox, CpsNumberBox)) CpsNumberBox.Value = 1000d / _engine.IntervalMilliseconds;
		}
		finally {
			_isSynchronizing = false;
		}
	}

	private void OnToggleRequested() => ClickingEnabledToggleSwitch.IsOn = !ClickingEnabledToggleSwitch.IsOn;

	private void OnClickingEnabledToggled(object sender, RoutedEventArgs e) {
		if (_isSynchronizing) return;
		_engine.Enabled = ClickingEnabledToggleSwitch.IsOn;
		UpdateClickingStatus();
	}

	private void OnClickingFailed(string message) {
		ClickingEnabledToggleSwitch.IsOn = false;
		StatusTextBlock.Text = Localization.Get("Blocked");
		ToolTipService.SetToolTip(StatusTextBlock, message);
	}

	private void UpdateClickingStatus() {
		bool isEnabled = ClickingEnabledToggleSwitch.IsOn;
		StatusTextBlock.Text = Localization.Get(isEnabled ? "Enabled" : "Disabled");
		ToolTipService.SetToolTip(StatusTextBlock, null);

		string resourceKey = isEnabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush";
		if (Application.Current.Resources.TryGetValue(resourceKey, out object brushObject) && brushObject is Brush brush) {
			StatusIndicator.Fill = brush;
		}
	}
}
