using GamiAutoClicker.WindowManager;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Windows.UI;

namespace GamiAutoClicker;

public sealed partial class SettingsWindow : ThemedWindow {
	public override WindowKey WindowKey => WindowKey.Settings;

	public SettingsWindow() {
		InitializeComponent();
		UpdateSwitches(WindowController.ThemeSettings);
	}

	private void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Acrylic":
				ThemeHelper.SetType(ThemeType.Acrylic);
				ThemeHelper.SetAcrylicKind(DesktopAcrylicKind.Base);
				break;
			case "AcrylicThin":
				ThemeHelper.SetType(ThemeType.Acrylic);
				ThemeHelper.SetAcrylicKind(DesktopAcrylicKind.Thin);
				break;
			case "Mica":
				ThemeHelper.SetType(ThemeType.Mica);
				ThemeHelper.SetMicaKind(MicaKind.Base);
				break;
			case "MicaAlt":
				ThemeHelper.SetType(ThemeType.Mica);
				ThemeHelper.SetMicaKind(MicaKind.BaseAlt);
				break;
			default:
				break;
		}
	}

	private void OnThemeChange(object sender, SelectionChangedEventArgs e) {
		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Dark":
				ThemeHelper.SetTheme(SystemBackdropTheme.Dark);
				break;
			case "Light":
				ThemeHelper.SetTheme(SystemBackdropTheme.Light);
				break;
			case "Default":
				ThemeHelper.SetTheme(SystemBackdropTheme.Default);
				break;
			default:
				break;
		}
	}

	private void OnOverridesChange(object sender, RoutedEventArgs e) {
		var toggleSwitch = (ToggleSwitch)sender;
		ThemeHelper.SetOverrides(toggleSwitch.IsOn);

		UpdateSwitches(WindowController.ThemeSettings);
	}

	private void OnFallbackColorChange(object sender, Color color) {
		ThemeHelper.SetFallbackColor(color);
	}

	private void OnTintColorChange(object sender, Color color) {
		ThemeHelper.SetTintColor(color);
	}

	private void OnTintOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetTintOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	private void OnLuminosityOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetLuminosityOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	public void UpdateSwitches(ThemeSettings settings) {
		BackdropMaterialComboBox.SelectedItem = settings.type.ToString();
		ThemeComboBox.SelectedItem = settings.theme.ToString();
		OverrideDefaultsToggleSwitch.IsOn = settings.shouldOverride;
		FallbackColorPicker.SelectedColor = settings.fallbackColor;
		TintColorPicker.SelectedColor = settings.tintColor;
		TintOpacitySlider.Value = settings.tintOpacity;
		LuminosityOpacitySlider.Value = settings.luminosityOpacity;
	}
}
