using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Settings;

// Locale participates in props equality so translated text refreshes.
internal sealed record AppearanceSummaryCardProps(
	EasyWindows.ThemeSettings Theme,
	string Locale);

internal sealed class AppearanceSummaryCard : Component<AppearanceSummaryCardProps> {
	public override Element Render() {
		var theme = Props.Theme;
		string colorMode = theme.Theme == SystemBackdropTheme.Default
			? Localization.Get("FollowsWindowscolormode")
			: Localization.Format("ColorModeSummary", Localization.Get(theme.Theme.ToString()));
		string overrides = Localization.Get(theme.ShouldOverride ? "Customcolortuningisactive" : "Usingsystemmaterialdefaults");
		var preview = Border(VStack(3,
			BodyStrong(AppearanceText.MaterialName(theme.BackdropMaterial)),
			SettingsCaption(colorMode),
			SettingsCaption(overrides)
		)).Padding(10).Background(new ThemeRef("LayerFillColorDefaultBrush"))
			.BorderBrush(new ThemeRef("CardStrokeColorDefaultBrush")).BorderThickness(1).CornerRadius(8);

		return SettingsCard(
			SectionHeading(Localization.Get("AppearancePage_Localized3.Text"), "\uE790"),
			SettingsCaption(Localization.Get("AppearancePage_Localized4.Text")),
			preview);
	}
}
