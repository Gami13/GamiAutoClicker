using Gami;
using GamiAutoClicker.Components;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.UI;

namespace GamiAutoClicker;

internal sealed partial class SettingsWindow : Window {
	private bool _isSynchronizing;

	public SettingsWindow() {
		InitializeComponent();
		UpdateSwitches(EasyWindows.Theme);
	}

	private void OnRootGridLoaded(object sender, RoutedEventArgs e) {
		UpdateResponsiveLayout(RootGrid.ActualWidth);
	}

	private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e) {
		UpdateResponsiveLayout(e.NewSize.Width);
	}

	private void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing) return;

		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Acrylic":
				EasyWindows.SetBackdropMaterial(EasyWindows.BackdropMaterial.Acrylic);
				break;
			case "AcrylicThin":
				EasyWindows.SetBackdropMaterial(EasyWindows.BackdropMaterial.AcrylicThin);
				break;
			case "Mica":
				EasyWindows.SetBackdropMaterial(EasyWindows.BackdropMaterial.Mica);
				break;
			case "MicaAlt":
				EasyWindows.SetBackdropMaterial(EasyWindows.BackdropMaterial.MicaAlt);
				break;
			default:
				break;
		}

		UpdateAppearanceSummary(EasyWindows.Theme);
	}

	private void OnThemeChange(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing) return;

		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Dark":
				EasyWindows.ApplyTheme(SystemBackdropTheme.Dark);
				break;
			case "Light":
				EasyWindows.ApplyTheme(SystemBackdropTheme.Light);
				break;
			case "Default":
				EasyWindows.ApplyTheme(SystemBackdropTheme.Default);
				break;
			default:
				break;
		}

		UpdateAppearanceSummary(EasyWindows.Theme);
	}

	private void OnOverridesChange(object sender, RoutedEventArgs e) {
		if (_isSynchronizing) return;

		var toggleSwitch = (ToggleSwitch)sender;
		EasyWindows.SetOverrides(toggleSwitch.IsOn);
		UpdateSwitches(EasyWindows.Theme);
	}

#pragma warning disable CA1822 // XAML event handlers must be instance methods
	private void OnFallbackColorChange(ColorPicker sender, ColorChangedEventArgs args) {
		if (_isSynchronizing) return;

		EasyWindows.SetFallbackColor(args.NewColor);
	}
	private void OnTintColorChange(ColorPicker sender, ColorChangedEventArgs args) {
		if (_isSynchronizing) return;

		EasyWindows.SetTintColor(args.NewColor);
	}
#pragma warning restore CA1822

	private void OnTintOpacityChange(object sender, RoutedEventArgs e) {
		if (_isSynchronizing || sender is not LabeledPercentageSlider slider) return;

		EasyWindows.SetTintOpacity((float)slider.Percentage);
	}

	private void OnLuminosityOpacityChange(object sender, RoutedEventArgs e) {
		if (_isSynchronizing || sender is not LabeledPercentageSlider slider) return;

		EasyWindows.SetLuminosityOpacity((float)slider.Percentage);
	}

	private void OnRestoreDefaultsClick(object sender, RoutedEventArgs e) {
		EasyWindows.RestoreThemeDefaults();
		UpdateSwitches(EasyWindows.Theme);
	}

	public void UpdateSwitches(EasyWindows.ThemeSettings settings) {
		_isSynchronizing = true;
		try {
			BackdropMaterialComboBox.SelectedItem = settings.backdropMaterial.ToString();
			ThemeComboBox.SelectedItem = settings.theme.ToString();
			OverrideDefaultsToggleSwitch.IsOn = settings.shouldOverride;
			FallbackColorPicker.Color = settings.fallbackColor;
			TintColorPicker.Color = settings.tintColor;
			TintOpacitySlider.Percentage = settings.tintOpacity;
			LuminosityOpacitySlider.Percentage = settings.luminosityOpacity;
		}
		finally {
			_isSynchronizing = false;
		}

		UpdateAdvancedControlState(settings.shouldOverride);
		UpdateAppearanceSummary(settings);
	}

	private void UpdateAdvancedControlState(bool isEnabled) {
		AdvancedControlsPanel.Opacity = isEnabled ? 1 : 0.55;
		FallbackColorPicker.IsEnabled = isEnabled;
		TintColorPicker.IsEnabled = isEnabled;
		TintOpacitySlider.IsEnabled = isEnabled;
		LuminosityOpacitySlider.IsEnabled = isEnabled;
		AdvancedControlsHint.Text = isEnabled
			? "Custom colors are applied immediately to every open window."
			: "Enable custom backdrop colors to edit these values.";
	}

	private void UpdateAppearanceSummary(EasyWindows.ThemeSettings settings) {
		PreviewMaterialText.Text = FormatBackdropMaterial(settings.backdropMaterial);
		PreviewThemeText.Text = settings.theme switch {
			SystemBackdropTheme.Default => "Follows Windows color mode",
			_ => $"{settings.theme} color mode"
		};
		PreviewOverrideText.Text = settings.shouldOverride
			? "Custom color tuning is active"
			: "Using system material defaults";
	}

	private static string FormatBackdropMaterial(EasyWindows.BackdropMaterial material) => material switch {
		EasyWindows.BackdropMaterial.MicaAlt => "Mica Alt",
		EasyWindows.BackdropMaterial.AcrylicThin => "Thin Acrylic",
		_ => material.ToString()
	};

	private void UpdateResponsiveLayout(double width) {
		bool useTwoSettingsColumns = width >= 820;
		SettingsLeftColumn.Width = new GridLength(1, GridUnitType.Star);
		SettingsRightColumn.Width = useTwoSettingsColumns
			? new GridLength(1, GridUnitType.Star)
			: new GridLength(0);
		SettingsPreviewRow.Height = GridLength.Auto;
		SettingsBaseRow.Height = GridLength.Auto;
		SettingsAdvancedRow.Height = useTwoSettingsColumns
			? new GridLength(0)
			: GridLength.Auto;
		Grid.SetColumn(AdvancedTuningCard, useTwoSettingsColumns ? 1 : 0);
		Grid.SetRow(AdvancedTuningCard, useTwoSettingsColumns ? 0 : 2);
		Grid.SetRowSpan(AdvancedTuningCard, useTwoSettingsColumns ? 2 : 1);

		bool useTwoColorPickers = width >= 620;
		FallbackColorColumn.Width = new GridLength(1, GridUnitType.Star);
		TintColorColumn.Width = useTwoColorPickers
			? new GridLength(1, GridUnitType.Star)
			: new GridLength(0);
		TintColorRow.Height = useTwoColorPickers
			? new GridLength(0)
			: GridLength.Auto;
		Grid.SetColumn(TintColorPicker, useTwoColorPickers ? 1 : 0);
		Grid.SetRow(TintColorPicker, useTwoColorPickers ? 0 : 1);
	}
}
