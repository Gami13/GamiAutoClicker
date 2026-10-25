using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed class GeneralPage : Component
{
    public override Element Render()
    {
        var (toggle, setToggle) = UseState(GeneralSettings.ToggleKey);
        var (hold, setHold) = UseState(GeneralSettings.HoldKey);
        var (language, setLanguage) = UseState(GeneralSettings.Language);
        var (error, setError) = UseState<string?>(null);
        string[] keyLabels = GeneralSettings.Keys.Select(GeneralSettings.KeyLabel).ToArray();
        string[] languageLabels = Localization.Languages.Select(Localization.LanguageLabel).ToArray();

        void Apply(VirtualKey newToggle, VirtualKey newHold, string newLanguage)
        {
            if (GeneralSettings.TryApply(newToggle, newHold, newLanguage, out string message))
                setError(null);
            else
                setError(message);
            setToggle(GeneralSettings.ToggleKey);
            setHold(GeneralSettings.HoldKey);
            setLanguage(GeneralSettings.Language);
        }

        return VStack(12,
            Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
                VStack(2,
                    Subtitle(Localization.Get("GeneralPage_Localized1.Text")),
                    Caption(Localization.Get("GeneralPage_Localized2.Text"))).Grid(0, 0),
                Button(Localization.Get("Restoredefaults"), () => Apply(VirtualKey.F8, VirtualKey.XButton1, "system"))
                    .AutomationName(Localization.Get("Restoredefaults")).Grid(0, 1)),
            ScrollView(VStack(12,
                error is not null ? InfoBar(null, error) with { Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error } : null,
                Card(VStack(8,
                    BodyStrong(Localization.Get("GeneralPage_Localized3.Text")),
                    Caption(Localization.Get("GeneralPage_Localized4.Text")),
                    TextBlock(Localization.Get("GeneralPage_ToggleKeyComboBox.Header")),
                    ComboBox(keyLabels, Array.IndexOf(GeneralSettings.Keys, toggle), index =>
                    {
                        if ((uint)index < GeneralSettings.Keys.Length)
                            Apply(GeneralSettings.Keys[index], hold, language);
                    }).AutomationName(Localization.Get("GeneralPage_ToggleKeyComboBox.Header")),
                    Caption(Localization.Get("GeneralPage_Localized5.Text")),
                    TextBlock(Localization.Get("GeneralPage_HoldKeyComboBox.Header")),
                    ComboBox(keyLabels, Array.IndexOf(GeneralSettings.Keys, hold), index =>
                    {
                        if ((uint)index < GeneralSettings.Keys.Length)
                            Apply(toggle, GeneralSettings.Keys[index], language);
                    }).AutomationName(Localization.Get("GeneralPage_HoldKeyComboBox.Header")),
                    Caption(Localization.Get("GeneralPage_Localized6.Text")),
                    Caption(Localization.Get("GeneralPage_Localized7.Text"))
                )),
                Card(VStack(8,
                    BodyStrong(Localization.Get("GeneralPage_Localized8.Text")),
                    TextBlock(Localization.Get("GeneralPage_LanguageComboBox.Header")),
                    ComboBox(languageLabels, Array.IndexOf(Localization.Languages, language), index =>
                    {
                        if ((uint)index < Localization.Languages.Length)
                            Apply(toggle, hold, Localization.Languages[index]);
                    }).AutomationName(Localization.Get("GeneralPage_LanguageComboBox.Header")),
                    Caption(Localization.Get("GeneralPage_Localized9.Text"))
                ))
            ))
        ).Padding(20);
    }
}
