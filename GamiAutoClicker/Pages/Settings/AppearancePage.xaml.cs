using Gami;
using System;
using System.Linq;
using GamiAutoClicker.Components;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker.Pages.Settings;

internal sealed partial class AppearancePage : Page {

	private bool _isSynchronizing;

	public AppearancePage() {
		InitializeComponent();
		BackdropMaterialComboBox.ItemsSource = Enum.GetValues<EasyWindows.BackdropMaterial>().Cast<object>().ToArray();
		ThemeComboBox.ItemsSource = new object[] { SystemBackdropTheme.Dark, SystemBackdropTheme.Light, SystemBackdropTheme.Default };
		RefreshControls(EasyWindows.Theme);
	}

	private void OnRootGridLoaded(object sender, RoutedEventArgs e) {
		UpdateResponsiveLayout(RootGrid.ActualWidth);
	}

	private void OnRootGridSizeChanged(object sender, SizeChangedEventArgs e) {
		UpdateResponsiveLayout(e.NewSize.Width);
	}

	private void OnMaterialChange(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing ||
			BackdropMaterialComboBox.SelectedItem is not EasyWindows.BackdropMaterial material) return;

		EasyWindows.SetBackdropMaterial(material);
		UpdateAppearanceSummary(EasyWindows.Theme);
	}

	private void OnThemeChange(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing || ThemeComboBox.SelectedItem is not SystemBackdropTheme theme) return;

		EasyWindows.ApplyTheme(theme);
		UpdateAppearanceSummary(EasyWindows.Theme);
	}

	private void OnOverridesChange(object sender, RoutedEventArgs e) {
		if (_isSynchronizing) return;

		var toggleSwitch = (ToggleSwitch)sender;
		EasyWindows.SetOverrides(toggleSwitch.IsOn);
		RefreshControls(EasyWindows.Theme);
	}

	private void OnFallbackColorChange(ColorPicker sender, ColorChangedEventArgs args) {
		if (_isSynchronizing) return;

		EasyWindows.SetFallbackColor(args.NewColor);
	}
	private void OnTintColorChange(ColorPicker sender, ColorChangedEventArgs args) {
		if (_isSynchronizing) return;

		EasyWindows.SetTintColor(args.NewColor);
	}

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
		RefreshControls(EasyWindows.Theme);
	}

	private void RefreshControls(EasyWindows.ThemeSettings settings) {
		_isSynchronizing = true;
		try {
			BackdropMaterialComboBox.SelectedItem = settings.BackdropMaterial;
			ThemeComboBox.SelectedItem = settings.Theme;
			OverrideDefaultsToggleSwitch.IsOn = settings.ShouldOverride;
			FallbackColorPicker.Color = settings.FallbackColor;
			TintColorPicker.Color = settings.TintColor;
			TintOpacitySlider.Percentage = settings.TintOpacity;
			LuminosityOpacitySlider.Percentage = settings.LuminosityOpacity;
		}
		finally {
			_isSynchronizing = false;
		}

		UpdateAdvancedControlState(settings.ShouldOverride);
		UpdateAppearanceSummary(settings);
	}

	private void UpdateAdvancedControlState(bool isEnabled) {
		AdvancedControlsPanel.Opacity = isEnabled ? 1 : 0.55;
		FallbackColorPicker.IsEnabled = isEnabled;
		TintColorPicker.IsEnabled = isEnabled;
		TintOpacitySlider.IsEnabled = isEnabled;
		LuminosityOpacitySlider.IsEnabled = isEnabled;

	}

	private void UpdateAppearanceSummary(EasyWindows.ThemeSettings settings) {
		PreviewMaterialText.Text = FormatBackdropMaterial(settings.BackdropMaterial);
		PreviewThemeText.Text = settings.Theme switch {
			SystemBackdropTheme.Default => "Follows Windows color mode",
			_ => $"{settings.Theme} color mode"
		};
		PreviewOverrideText.Text = settings.ShouldOverride
			? "Custom color tuning is active"
			: "Using system material defaults";
	}

	private static string FormatBackdropMaterial(EasyWindows.BackdropMaterial material) => material switch {
		EasyWindows.BackdropMaterial.MicaAlt => "Mica Alt",
		EasyWindows.BackdropMaterial.AcrylicThin => "Thin Acrylic",
		_ => material.ToString()
	};

	private const double RestoreDefaultsBreakpoint = 470;
	private const double TwoSettingsColumnsWidth = 590;
	private const double TwoColorPickersWidth = 820;

	private void UpdateResponsiveLayout(double width) {
		RestoreDefaultsControl.IsExpanded = width >= RestoreDefaultsBreakpoint;
		bool useTwoSettingsColumns = width >= TwoSettingsColumnsWidth;
		SettingsRightColumn.Width = useTwoSettingsColumns
			? new GridLength(1, GridUnitType.Star)
			: new GridLength(0);
		SettingsAdvancedRow.Height = useTwoSettingsColumns
			? new GridLength(0)
			: GridLength.Auto;
		Grid.SetColumn(AdvancedTuningCard, useTwoSettingsColumns ? 1 : 0);
		Grid.SetRow(AdvancedTuningCard, useTwoSettingsColumns ? 0 : 2);
		Grid.SetRowSpan(AdvancedTuningCard, useTwoSettingsColumns ? 2 : 1);

		bool useTwoColorPickers = width >= TwoColorPickersWidth;
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
