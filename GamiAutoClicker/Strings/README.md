# Localization and content reload

The app supports Follow Windows, English, and Polski. The preference is saved to
`%LOCALAPPDATA%\GamiAutoClicker\language.txt`, including in unpackaged builds.
Invalid or unreadable preferences use Follow Windows.

Static XAML text uses native `x:Uid`:

```xml
<TextBlock x:Uid="IntervalLabel" />
```

Each language's `Resources.resw` supplies `IntervalLabel.Text`. Existing IDs with
`MainWindow_` and `SettingsWindow_` prefixes remain stable even though the content
is now authored as pages. Dynamic messages and option lists use `Localization.Get`
and `Localization.Format` during initialization or when application state changes.
Preserve numbered placeholders such as `{0}` when translating sentences.

## Window ownership

EasyWindows creates the native window, title bar, and content host. The app registers
a factory that returns a fresh Page or UserControl:

```csharp
EasyWindows.RegisterWindow(WindowKey.Main, new EasyWindows.WindowOptions {
    ContentFactory = () => new MainPage(engine),
    Title = "Gami's AutoClicker",
    DefaultSize = new SizeInt32(440, 330)
});
```

`ReloadAllWindows()` calls each open controller's `ReloadContent()` on the UI
thread. The factory creates new content, which replaces the previous content in
the existing host. Window identity, bounds, shell, and backdrop survive. Closed
windows stay closed. Title and accessibility-name providers are reevaluated.

Content owns initialization of its controls from app state. EasyWindows does not
capture or restore control values, focus, navigation selection, or scroll position.
The settings page starts on Appearance after a reload. Theme settings and the
app-owned click engine survive; the new controls read their current values.

Root content that implements `IDisposable` is disposed on replacement and window
closure. Use this to detach subscriptions to long-lived services and release owned
resources, including any child resources requiring explicit cleanup. WindowOptions
`Closed` runs for actual window closure, not content reload. The main window's
close callback stops the app-owned engine and exits the application.

Language changes update `Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride`
and queue `ReloadAllWindows()` after the settings event commits. Fresh XAML loads
its native localized resources; no custom property traversal is required.

## Adding and verifying translations

1. Copy `Strings/en/Resources.resw` to `Strings/<language-tag>/Resources.resw` and translate values.
2. Add the tag to `Localization.Languages` and its native name to GeneralPage's language options, in matching order.
3. Add the language to `Package.appxmanifest`.
4. Verify long strings, custom controls, both settings layouts, and flow direction before adding a right-to-left language.

Build an unpackaged integration-test app with:

```powershell
dotnet build GamiAutoClicker/GamiAutoClicker.csproj -p:Platform=x64 -p:ReloadVerification=true -p:PublishAot=false -p:WindowsPackageType=None -p:EnforceCodeStyleInBuild=false
```

Launch the built executable. The opt-in test changes language through the actual
ComboBox, checks replacement and native localization, retained window identity and
engine/appearance state, input-pause cleanup, subscriptions, and closed windows.
It restores the original preference and appearance override setting, writes
`reload-verification.txt` beside the executable, and closes the app. Test code is
excluded from ordinary builds. The style override accommodates existing `IDE0008`
diagnostics elsewhere in the repository; it does not disable compiler errors.
