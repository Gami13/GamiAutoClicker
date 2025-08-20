using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Composition.SystemBackdrops;


namespace GamiAutoClicker;

public sealed partial class SettingsPage : Page {
	public SettingsPage() {
		InitializeComponent();

	}
	public void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
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

	public void OnThemeChange(object sender, SelectionChangedEventArgs e) {
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
	public void OnOverridesChange(object sender, RoutedEventArgs e) {
		var toggleSwitch = (ToggleSwitch)sender;
		ThemeHelper.SetOverrides(toggleSwitch.IsOn);

	}
	public void OnFallbackColorChange(object sender, ColorChangedEventArgs e) {
		ThemeHelper.SetFallbackColor(e.NewColor);
	}
	public void OnTintColorChange(object sender, ColorChangedEventArgs e) {
		ThemeHelper.SetTintColor(e.NewColor);
	}

	public void OnTintOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetTintOpacity((float)e.NewValue);
	}

	public void OnLuminosityOpacityChange(object sender, RangeBaseValueChangedEventArgs e) {
		ThemeHelper.SetLuminosityOpacity((float)e.NewValue);
	}

}