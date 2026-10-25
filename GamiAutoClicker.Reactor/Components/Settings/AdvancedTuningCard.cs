using Gami;
using GamiToolkit.Components;
using Windows.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Settings;

// Locale participates in props equality so translated text refreshes.
internal sealed record AdvancedTuningCardProps(
	EasyWindows.ThemeSettings Theme,
	bool WideColors,
	string Locale,
	Action<bool> OnOverridesChanged,
	Action<Color> OnFallbackChanged,
	Action<Color> OnTintChanged,
	Action<double> OnTintOpacityChanged,
	Action<double> OnLuminosityChanged);

internal sealed class AdvancedTuningCard : Component<AdvancedTuningCardProps> {
	public override Element Render() {
		var theme = Props.Theme;
		var fallbackColor = ColorPickerFlyoutButton.Create(
			Localization.Get("AppearancePage_FallbackColorPicker.Header"),
			theme.FallbackColor, Props.OnFallbackChanged);
		var tintColor = ColorPickerFlyoutButton.Create(
			Localization.Get("AppearancePage_TintColorPicker.Header"),
			theme.TintColor, Props.OnTintChanged);
		var colors = Grid(
			[GridSize.Star(), Props.WideColors ? GridSize.Star() : GridSize.Px(0)],
			[GridSize.Auto, GridSize.Auto],
			fallbackColor.Grid(0, 0),
			tintColor.Grid(Props.WideColors ? 0 : 1, Props.WideColors ? 1 : 0)
		).Set(grid => {
			grid.ColumnSpacing = 10;
			grid.RowSpacing = 10;
		});
		var tuning = VStack(10,
			colors,
			LabeledPercentageSlider(Localization.Get("AppearancePage_TintOpacitySlider.Header"), theme.TintOpacity, Props.OnTintOpacityChanged),
			LabeledPercentageSlider(Localization.Get("AppearancePage_LuminosityOpacitySlider.Header"), theme.LuminosityOpacity, Props.OnLuminosityChanged)
		).IsEnabled(theme.ShouldOverride).Opacity(theme.ShouldOverride ? 1 : 0.55);

		return SettingsCard(
			BodyStrong(Localization.Get("AppearancePage_Localized9.Text")),
			SettingsCaption(Localization.Get("AppearancePage_Localized10.Text")),
			ToggleSwitch(theme.ShouldOverride, Props.OnOverridesChanged, header: Localization.Get("AppearancePage_OverrideDefaultsToggleSwitch.Header")),
			tuning);
	}
}
