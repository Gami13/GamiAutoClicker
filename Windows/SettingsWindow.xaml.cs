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
		UpdateSwitches(EasyWindows.Manager.ThemeSettings);
	}

	private void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
		var comboBox = (ComboBox)sender;
		var selectedItem = comboBox.SelectedItem;

		switch (selectedItem) {
			case "Acrylic":
				EasyWindows.Manager.SetType(EasyWindows.ThemeType.Acrylic);
				EasyWindows.Manager.SetAcrylicKind(DesktopAcrylicKind.Base);
				break;
			case "AcrylicThin":
				EasyWindows.Manager.SetType(EasyWindows.ThemeType.Acrylic);
				EasyWindows.Manager.SetAcrylicKind(DesktopAcrylicKind.Thin);
				break;
			case "Mica":
				EasyWindows.Manager.SetType(EasyWindows.ThemeType.Mica);
				EasyWindows.Manager.SetMicaKind(MicaKind.Base);
				break;
			case "MicaAlt":
				EasyWindows.Manager.SetType(EasyWindows.ThemeType.Mica);
				EasyWindows.Manager.SetMicaKind(MicaKind.BaseAlt);
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
				EasyWindows.Manager.SetTheme(SystemBackdropTheme.Dark);
				break;
			case "Light":
				EasyWindows.Manager.SetTheme(SystemBackdropTheme.Light);
				break;
			case "Default":
				EasyWindows.Manager.SetTheme(SystemBackdropTheme.Default);
				break;
			default:
				break;
		}
	}

	private void OnOverridesChange(object sender, RoutedEventArgs e) {
		var toggleSwitch = (ToggleSwitch)sender;
		EasyWindows.Manager.SetOverrides(toggleSwitch.IsOn);

		UpdateSwitches(EasyWindows.Manager.ThemeSettings);
	}

#pragma warning disable CA1822 // XAML event handlers must be instance methods
	private void OnFallbackColorChange(object sender, Color color) {
		EasyWindows.Manager.SetFallbackColor(color);
	}
	private void OnTintColorChange(object sender, Color color) {
		EasyWindows.Manager.SetTintColor(color);
	}
#pragma warning restore CA1822

	private void OnTintOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		EasyWindows.Manager.SetTintOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	private void OnLuminosityOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		EasyWindows.Manager.SetLuminosityOpacity(Math.Clamp((float)e.NewValue, 0f, 1f));
	}

	public void UpdateSwitches(EasyWindows.ThemeSettings settings) {
		BackdropMaterialComboBox.SelectedItem = settings.type.ToString();
		ThemeComboBox.SelectedItem = settings.theme.ToString();
		OverrideDefaultsToggleSwitch.IsOn = settings.shouldOverride;
		FallbackColorPicker.SelectedColor = settings.fallbackColor;
		TintColorPicker.SelectedColor = settings.tintColor;
		TintOpacitySlider.Value = settings.tintOpacity;
		LuminosityOpacitySlider.Value = settings.luminosityOpacity;
	}
}
