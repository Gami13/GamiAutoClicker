using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Composition.SystemBackdrops;


namespace GamiAutoClicker;

public sealed partial class SettingsWindow : Window {
	public SettingsWindow() {
		InitializeComponent();

		//UpdateSwitches();


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

	}
	private void OnFallbackColorChange(object sender, ColorChangedEventArgs e) {
		ThemeHelper.SetFallbackColor(e.NewColor);
	}
	private void OnTintColorChange(object sender, ColorChangedEventArgs e) {
		ThemeHelper.SetTintColor(e.NewColor);
	}

	private void OnTintOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetTintOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	private void OnLuminosityOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetLuminosityOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	public void UpdateSwitches() {
		//BackdropMaterialComboBox.SelectedItem = MainWindow.themeSettings.type.ToString();
		//ThemeComboBox.SelectedItem = MainWindow.themeSettings.theme.ToString();
		//OverrideDefaultsToggleSwitch.IsOn = MainWindow.themeSettings.shouldOverride;
		//FallbackColorPicker.Color = MainWindow.themeSettings.fallbackColor;
		//TintColorPicker.Color = MainWindow.themeSettings.tintColor;
		//TintOpacitySlider.Value = MainWindow.themeSettings.tintOpacity;
		//LuminosityOpacitySlider.Value = MainWindow.themeSettings.luminosityOpacity;
	}


}