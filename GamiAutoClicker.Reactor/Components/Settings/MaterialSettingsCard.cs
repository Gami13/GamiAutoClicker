using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Settings;

// Locale participates in props equality so translated text refreshes.
internal sealed record MaterialSettingsCardProps(
	string[] Materials,
	int MaterialIndex,
	string[] Themes,
	int ThemeIndex,
	string Locale,
	Action<int> OnMaterialChanged,
	Action<int> OnThemeChanged);

internal sealed class MaterialSettingsCard : Component<MaterialSettingsCardProps> {
	public override Element Render() => SettingsCard(
		BodyStrong(Localization.Get("AppearancePage_Localized5.Text")),
		SettingsCaption(Localization.Get("AppearancePage_Localized6.Text")),
		SettingsSelector(
			label: Localization.Get("AppearancePage_BackdropMaterialComboBox.Header"),
			choices: Props.Materials,
			selectedIndex: Props.MaterialIndex,
			changed: Props.OnMaterialChanged),
		SettingsCaption(Localization.Get("AppearancePage_Localized7.Text")),
		SettingsSelector(
			label: Localization.Get("AppearancePage_ThemeComboBox.Header"),
			choices: Props.Themes,
			selectedIndex: Props.ThemeIndex,
			changed: Props.OnThemeChanged),
		SettingsCaption(Localization.Get("AppearancePage_Localized8.Text"))
	);
}
