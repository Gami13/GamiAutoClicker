using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;
using GamiToolkit.Components;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker;

internal sealed class GeneralPage : ResponsiveComponent {
	public override Element Render() {
		double width = UseContentWidth();
		bool wide = width >= 590;
		var (toggle, setToggle) = UseState(GeneralSettings.ToggleKey);
		var (hold, setHold) = UseState(GeneralSettings.HoldKey);
		var (language, setLanguage) = UseState(GeneralSettings.Language);
		var (error, setError) = UseState<string?>(null);
		var (attempt, setAttempt) = UseState(0);
		string[] keyLabels = GeneralSettings.Keys.Select(GeneralSettings.KeyLabel).ToArray();
		string[] languageLabels = Localization.Languages.Select(Localization.LanguageLabel).ToArray();

		void Apply(VirtualKey newToggle, VirtualKey newHold, string newLanguage) {
			if (GeneralSettings.TryApply(newToggle, newHold, newLanguage, out string message))
				setError(null);
			else
				setError(message);
			setToggle(GeneralSettings.ToggleKey);
			setHold(GeneralSettings.HoldKey);
			setLanguage(GeneralSettings.Language);
			// A rejected edit must restore the native selection even when model values did not change.
			setAttempt(attempt + 1);
		}

		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Star()],
			Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
				VStack(2,
					Subtitle(Localization.Get("GeneralPage_Localized1.Text")),
					SettingsCaption(Localization.Get("GeneralPage_Localized2.Text"))).Grid(0, 0),
				RestoreDefaultsButton(Localization.Get("Restoredefaults"), width >= 470, () => Apply(VirtualKey.F8, VirtualKey.XButton1, "system")).Grid(0, 1)
			).Margin(new Microsoft.UI.Xaml.Thickness(24, 16, 24, 12)).Set(g => g.ColumnSpacing = 16),
			ScrollView(VStack(12,
				error is not null ? InfoBar(null, error) with { Key = "validation-" + attempt, Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error } : null,
				Grid([GridSize.Star(), wide ? GridSize.Star() : GridSize.Px(0)], [GridSize.Auto, GridSize.Auto],
				SettingsCard(
					HStack(8, TextBlock("\uE765").FontFamily("Segoe Fluent Icons").FontSize(18).Foreground(new ThemeRef("AccentTextFillColorPrimaryBrush")), BodyStrong(Localization.Get("GeneralPage_Localized3.Text"))),
					SettingsCaption(Localization.Get("GeneralPage_Localized4.Text")),
					ComboBox(keyLabels, Array.IndexOf(GeneralSettings.Keys, toggle), index => {
						if ((uint)index < GeneralSettings.Keys.Length)
							Apply(GeneralSettings.Keys[index], hold, language);
					}).AutomationName(Localization.Get("GeneralPage_ToggleKeyComboBox.Header")).HorizontalAlignment(Microsoft.UI.Xaml.HorizontalAlignment.Stretch).Set(c => { c.Header = Localization.Get("GeneralPage_ToggleKeyComboBox.Header"); }),
					SettingsCaption(Localization.Get("GeneralPage_Localized5.Text")),
					ComboBox(keyLabels, Array.IndexOf(GeneralSettings.Keys, hold), index => {
						if ((uint)index < GeneralSettings.Keys.Length)
							Apply(toggle, GeneralSettings.Keys[index], language);
					}).AutomationName(Localization.Get("GeneralPage_HoldKeyComboBox.Header")).HorizontalAlignment(Microsoft.UI.Xaml.HorizontalAlignment.Stretch).Set(c => { c.Header = Localization.Get("GeneralPage_HoldKeyComboBox.Header"); }),
					SettingsCaption(Localization.Get("GeneralPage_Localized6.Text")),
					SettingsCaption(Localization.Get("GeneralPage_Localized7.Text"))
				).Grid(0, 0),
				SettingsCard(
					HStack(8, TextBlock("\uF2B7").FontFamily("Segoe Fluent Icons").FontSize(18).Foreground(new ThemeRef("AccentTextFillColorPrimaryBrush")), BodyStrong(Localization.Get("GeneralPage_Localized8.Text"))),
					ComboBox(languageLabels, Array.IndexOf(Localization.Languages, language), index => {
						if ((uint)index < Localization.Languages.Length)
							Apply(toggle, hold, Localization.Languages[index]);
					}).AutomationName(Localization.Get("GeneralPage_LanguageComboBox.Header")).HorizontalAlignment(Microsoft.UI.Xaml.HorizontalAlignment.Stretch).Set(c => { c.Header = Localization.Get("GeneralPage_LanguageComboBox.Header"); }),
					SettingsCaption(Localization.Get("GeneralPage_Localized9.Text"))
				).Grid(wide ? 0 : 1, wide ? 1 : 0)
				).Set(g => { g.ColumnSpacing = 12; g.RowSpacing = 12; })
			).Margin(new Microsoft.UI.Xaml.Thickness(24, 0, 24, 8))).Set(view => {
				view.HorizontalScrollMode = Microsoft.UI.Xaml.Controls.ScrollingScrollMode.Disabled;
				view.HorizontalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollingScrollBarVisibility.Hidden;
				view.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollingScrollBarVisibility.Auto;
			}).Grid(1, 0)
		).Set(CaptureRoot);
	}
}
