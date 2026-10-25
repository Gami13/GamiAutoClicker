using Gami;
using Microsoft.UI.Reactor;

namespace GamiAutoClicker;

internal static class AppState
{
    internal static ClickEngine Engine { get; } = new();
    internal static ReactorAppearance Appearance { get; } = new();
    internal static SettingsWindowState SettingsWindow { get; } = new();

    internal static void Initialize()
    {
        SettingsStore.Load();
        Engine.IdlePollIntervalMilliseconds = 25;
        Engine.ToggleKey = GeneralSettings.ToggleKey;
        Engine.HoldKey = GeneralSettings.HoldKey;
    }

    internal static void OpenSettings()
    {
        ReactorWindow window = ReactorApp.OpenWindow(new WindowSpec
        {
            Key = new WindowKey("settings"),
            Title = Localization.Get("Settings"),
            Width = 860,
            Height = 600,
            MinWidth = 434,
            MinHeight = 320,
            MaxWidth = 1110,
            MaxHeight = 1440,
            ResizeMode = WindowResizeMode.CanResize,
            IsMinimizable = true,
            IsMaximizable = true
        }, () => new SettingsView());
        window.Activate();
    }
}

internal sealed class SettingsWindowState
{
    internal SettingsSection SelectedSection { get; set; } = SettingsSection.Appearance;
    internal bool IsPaneOpen { get; set; } = true;
}

internal enum SettingsSection { Appearance, General }
