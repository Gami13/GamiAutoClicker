using Gami;
using GamiAutoClicker.Components.Settings;
using Microsoft.UI.Composition.SystemBackdrops;

namespace GamiAutoClicker;

internal sealed partial class AppearancePage {
	private static readonly EasyWindows.BackdropMaterial[] Materials = Enum.GetValues<EasyWindows.BackdropMaterial>();
	private static readonly SystemBackdropTheme[] Themes = [SystemBackdropTheme.Dark, SystemBackdropTheme.Light, SystemBackdropTheme.Default];

	private AppearanceBindings UseBindings(bool wideColors) {
		var (_, refresh) = UseState(0);
		UseEffect(() => {
			int version = 0;
			void Changed(object? sender, EventArgs args) => refresh(++version);
			EasyWindows.ThemeChanged += Changed;
			return () => EasyWindows.ThemeChanged -= Changed;
		}, []);

		// Props retain a snapshot; the shared theme model mutates in place.
		var theme = EasyWindows.Theme with { };
		string locale = Localization.Culture.Name;
		void ChangeMaterial(int index) {
			if ((uint)index < Materials.Length) EasyWindows.SetBackdropMaterial(Materials[index]);
		}
		void ChangeTheme(int index) {
			if ((uint)index < Themes.Length) EasyWindows.ApplyTheme(Themes[index]);
		}
		void ChangeTintOpacity(double value) => EasyWindows.SetTintOpacity((float)value);
		void ChangeLuminosity(double value) => EasyWindows.SetLuminosityOpacity((float)value);

		var summary = new AppearanceSummaryCardProps(theme, locale);
		var material = new MaterialSettingsCardProps(
			Materials: Materials.Select(AppearanceText.MaterialName).ToArray(),
			MaterialIndex: Array.IndexOf(Materials, theme.BackdropMaterial),
			Themes: Themes.Select(value => Localization.Get(value.ToString())).ToArray(),
			ThemeIndex: Array.IndexOf(Themes, theme.Theme),
			Locale: locale,
			OnMaterialChanged: ChangeMaterial,
			OnThemeChanged: ChangeTheme);
		var tuning = new AdvancedTuningCardProps(
			Theme: theme,
			WideColors: wideColors,
			Locale: locale,
			OnOverridesChanged: EasyWindows.SetOverrides,
			OnFallbackChanged: EasyWindows.SetFallbackColor,
			OnTintChanged: EasyWindows.SetTintColor,
			OnTintOpacityChanged: ChangeTintOpacity,
			OnLuminosityChanged: ChangeLuminosity);

		return new AppearanceBindings(summary, material, tuning);
	}

	private sealed record AppearanceBindings(
		AppearanceSummaryCardProps Summary,
		MaterialSettingsCardProps Material,
		AdvancedTuningCardProps Tuning);
}
