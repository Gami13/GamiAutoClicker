using Gami;
using System;
using System.Linq;
using GamiAutoClicker.Components;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker.Pages.Settings;

internal sealed partial class AppearancePage : Page {
	// Keep enums in managed code; WinUI only sees a standard item with string content.
	private sealed partial class EnumComboBoxItem<T> : ComboBoxItem where T : struct, Enum {
		public T Value { get; }

		public EnumComboBoxItem(T value) {
			Value = value;
			Content = value.ToString();
		}
	}

	private bool _isSynchronizing;

	public AppearancePage() {
		InitializeComponent();
		foreach (var material in Enum.GetValues<EasyWindows.BackdropMaterial>())
			BackdropMaterialComboBox.Items.Add(new EnumComboBoxItem<EasyWindows.BackdropMaterial>(material));
		foreach (var theme in new[] { SystemBackdropTheme.Dark, SystemBackdropTheme.Light, SystemBackdropTheme.Default })
			ThemeComboBox.Items.Add(new EnumComboBoxItem<SystemBackdropTheme>(theme));
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
			BackdropMaterialComboBox.SelectedItem is not EnumComboBoxItem<EasyWindows.BackdropMaterial> item) return;

		EasyWindows.SetBackdropMaterial(item.Value);
		UpdateAppearanceSummary(EasyWindows.Theme);
	}

	private void OnThemeChange(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing || ThemeComboBox.SelectedItem is not EnumComboBoxItem<SystemBackdropTheme> item) return;

		EasyWindows.ApplyTheme(item.Value);
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
			BackdropMaterialComboBox.SelectedItem = BackdropMaterialComboBox.Items
				.Cast<EnumComboBoxItem<EasyWindows.BackdropMaterial>>().First(item => item.Value == settings.BackdropMaterial);
			ThemeComboBox.SelectedItem = ThemeComboBox.Items
				.Cast<EnumComboBoxItem<SystemBackdropTheme>>().First(item => item.Value == settings.Theme);
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

	private const double CompactRestoreButtonWidth = 470;
	private const double TwoSettingsColumnsWidth = 590;
	private const double TwoColorPickersWidth = 820;

	private void UpdateResponsiveLayout(double width) {
		bool isCompactButton = width < CompactRestoreButtonWidth;
		RestoreDefaultsButtonText.Visibility = isCompactButton ? Visibility.Collapsed : Visibility.Visible;
		RestoreDefaultsIcon.FontSize = isCompactButton ? 16 : 14;
		RestoreDefaultsButton.Width = isCompactButton ? 38 : double.NaN;
		RestoreDefaultsButton.Height = isCompactButton ? 38 : double.NaN;
		RestoreDefaultsButton.Padding = isCompactButton ? new Thickness(0) : new Thickness(11, 5, 11, 6);

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
