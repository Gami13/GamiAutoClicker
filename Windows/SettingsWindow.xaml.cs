using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using Windows.UI;

namespace GamiAutoClicker;

internal sealed partial class SettingsWindow : Window {

	public SettingsWindow() {
		InitializeComponent();
		UpdateSwitches(EasyWindows.Theme);
	}

	private void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
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
	}

	private void OnThemeChange(object sender, SelectionChangedEventArgs e) {
		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Dark":
				EasyWindows.SetTheme(SystemBackdropTheme.Dark);
				break;
			case "Light":
				EasyWindows.SetTheme(SystemBackdropTheme.Light);
				break;
			case "Default":
				EasyWindows.SetTheme(SystemBackdropTheme.Default);
				break;
			default:
				break;
		}
	}

	private void OnOverridesChange(object sender, RoutedEventArgs e) {
		var toggleSwitch = (ToggleSwitch)sender;
		EasyWindows.SetOverrides(toggleSwitch.IsOn);

		UpdateSwitches(EasyWindows.Theme);
	}

#pragma warning disable CA1822 // XAML event handlers must be instance methods
	private void OnFallbackColorChange(object sender, Color color) {
		EasyWindows.SetFallbackColor(color);
	}
	private void OnTintColorChange(object sender, Color color) {
		EasyWindows.SetTintColor(color);
	}
#pragma warning restore CA1822

	private void OnTintOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		EasyWindows.SetTintOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	private void OnLuminosityOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		EasyWindows.SetLuminosityOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	public void UpdateSwitches(EasyWindows.ThemeSettings settings) {
		BackdropMaterialComboBox.SelectedItem = settings.backdropMaterial.ToString();
		ThemeComboBox.SelectedItem = settings.theme.ToString();
		OverrideDefaultsToggleSwitch.IsOn = settings.shouldOverride;
		FallbackColorPicker.SelectedColor = settings.fallbackColor;
		TintColorPicker.SelectedColor = settings.tintColor;
		TintOpacitySlider.Value = settings.tintOpacity;
		LuminosityOpacitySlider.Value = settings.luminosityOpacity;
	}
}
