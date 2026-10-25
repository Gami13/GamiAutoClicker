using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Settings;

// Locale participates in props equality so translated text refreshes.
internal sealed record KeyBindingsCardProps(
	string[] Choices,
	int ToggleIndex,
	int HoldIndex,
	string Locale,
	int EditVersion,
	Action<int> OnToggleChanged,
	Action<int> OnHoldChanged);

internal sealed class KeyBindingsCard : Component<KeyBindingsCardProps> {
	public override Element Render() => SettingsCard(
		SectionHeading(Localization.Get("GeneralPage_Localized3.Text"), "\uE765"),
		SettingsCaption(Localization.Get("GeneralPage_Localized4.Text")),
		SettingsSelector(
			label: Localization.Get("GeneralPage_ToggleKeyComboBox.Header"),
			choices: Props.Choices,
			selectedIndex: Props.ToggleIndex,
			changed: Props.OnToggleChanged
		).HorizontalAlignment(HorizontalAlignment.Stretch),
		SettingsCaption(Localization.Get("GeneralPage_Localized5.Text")),
		SettingsSelector(
			label: Localization.Get("GeneralPage_HoldKeyComboBox.Header"),
			choices: Props.Choices,
			selectedIndex: Props.HoldIndex,
			changed: Props.OnHoldChanged
		).HorizontalAlignment(HorizontalAlignment.Stretch),
		SettingsCaption(Localization.Get("GeneralPage_Localized6.Text")),
		SettingsCaption(Localization.Get("GeneralPage_Localized7.Text"))
	);
}
