using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Settings;

// Locale participates in props equality so translated text refreshes.
internal sealed record LanguageCardProps(
	string[] Choices,
	int SelectedIndex,
	string Locale,
	int EditVersion,
	Action<int> OnChanged);

internal sealed class LanguageCard : Component<LanguageCardProps> {
	public override Element Render() => SettingsCard(
		SectionHeading(Localization.Get("GeneralPage_Localized8.Text"), "\uF2B7"),
		SettingsSelector(
			label: Localization.Get("GeneralPage_LanguageComboBox.Header"),
			choices: Props.Choices,
			selectedIndex: Props.SelectedIndex,
			changed: Props.OnChanged
		).HorizontalAlignment(HorizontalAlignment.Stretch),
		SettingsCaption(Localization.Get("GeneralPage_Localized9.Text"))
	);
}
