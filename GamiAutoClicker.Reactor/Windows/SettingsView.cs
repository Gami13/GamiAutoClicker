using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed class SettingsView : Component
{
    public override Element Render()
    {
        ReactorWindow window = UseWindow()!;
        SettingsWindowState retained = AppState.SettingsWindow;
        var (section, setSection) = UseState(retained.SelectedSection);
        var (paneOpen, setPaneOpen) = UseState(retained.IsPaneOpen);
        var (saveError, setSaveError) = UseState<string?>(null);
        var (_, setLanguageVersion) = UseState(0);

        UseEffect(() =>
        {
            AppState.Appearance.Attach(window);
            int languageVersion = 0;
            if (section == SettingsSection.General) GeneralSettings.BeginEditing(this);
            void LanguageChanged()
            {
                window.Update(window.Spec with { Title = Localization.Get("Settings") });
                setLanguageVersion(++languageVersion);
            }
            Localization.Changed += LanguageChanged;
            if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "settings" &&
                Environment.GetEnvironmentVariable("GAMI_PERF_READY_FILE") is { Length: > 0 } readyFile)
                File.WriteAllText(readyFile, "settings");
            return () =>
            {
                Localization.Changed -= LanguageChanged;
                GeneralSettings.EndEditing(this);
                AppState.Appearance.Detach(window);
                SettingsStore.DiscardChanges();
            };
        }, []);

        void ChangeSection(SettingsSection selected)
        {
            if (selected == SettingsSection.General) GeneralSettings.BeginEditing(this);
            else GeneralSettings.EndEditing(this);
            retained.SelectedSection = selected;
            setSection(selected);
        }

        void Save()
        {
            setSaveError(SettingsStore.TrySave(out string error) ? null : error);
        }

        return VStack(0,
            TitleBar(Localization.Get("Settings")),
            (NavigationView([
                    NavItem(Localization.Get("SettingsWindow_GeneralNavigationItem.Content"), "Setting", "general"),
                    NavItem(Localization.Get("SettingsWindow_AppearanceNavigationItem.Content"), "Edit", "appearance")
                ],
                VStack(0,
                    saveError is not null ? InfoBar(null, saveError) with { Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error } : null,
                    section == SettingsSection.General ? Component<GeneralPage>() : Component<AppearancePage>()
                )) with
                {
                    SelectedTag = section == SettingsSection.General ? "general" : "appearance",
                    OnSelectedTagChanged = tag => ChangeSection(tag == "general" ? SettingsSection.General : SettingsSection.Appearance),
                    IsPaneOpen = paneOpen,
                    OnPaneOpenChanged = open =>
                    {
                        retained.IsPaneOpen = open;
                        setPaneOpen(open);
                    },
                    IsSettingsVisible = false,
                    IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed,
                    IsTitleBarAutoPaddingEnabled = false,
                    OpenPaneLength = 150,
                    CompactModeThresholdWidth = 0,
                    ExpandedModeThresholdWidth = 750,
                    PaneFooter = Button(Localization.Get("SettingsSaveButton.Content"), Save)
                        .ToolTip(Localization.Get("SettingsSaveHint.Text"))
                        .AutomationName(Localization.Get("SettingsSaveButton.Content"))
                })
                .PaneTitle(Localization.Get("Settings"))
        );
    }
}
