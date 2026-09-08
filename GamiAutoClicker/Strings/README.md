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
Settings explicitly retains its selected section and pane-open state in the registration's state.
Theme settings and the app-owned click engine also survive; the new controls read
their current values.

For window-specific state, use the typed options:

```csharp
EasyWindows.RegisterWindow(WindowKey.Settings, new EasyWindows.WindowOptions<SettingsWindowState> {
    State = new SettingsWindowState(),
    ContentFactory = state => new SettingsPage(state),
    Title = "Settings",
    DefaultSize = new SizeInt32(860, 600)
});
```

`State` must be a reference type. The factory receives the same instance on every
opening and reload, including after the window has been closed. Its lifetime is
the registration's lifetime within the current app session; this does not save it
to disk. EasyWindows does not copy, serialize, or dispose state. The application
owns any resources it contains. Stateless windows keep using plain `WindowOptions`.

Content can look up state without constructor parameters:

```csharp
private readonly SettingsWindowState _state =
    EasyWindows.Windows[WindowKey.Settings].GetState<SettingsWindowState>();
```

`Windows` is a read-only view of all registrations, including closed windows.
Register a window before constructing content that looks it up. `GetState<T>()`
returns the same object, so assigning it to a variable and changing its properties
changes the registered state. Reassigning the local variable does not replace the
registered object. Missing keys throw `KeyNotFoundException`; missing state or an
incompatible requested type throws `InvalidOperationException`.

Settings accepts the factory's state in its constructor. Each registration can
therefore create the same page type with a different state instance. The lookup
remains available to other code, including while the window is closed.

`SettingsWindowState` is the model for Settings navigation: `SelectedSection` and
`IsPaneOpen`. It implements `INotifyPropertyChanged`; SettingsPage uses compiled
two-way `x:Bind` bindings so model changes update the live controls and UI changes
update the model. Change bound state on the window's UI thread. The selected-section
binding maps the enum to page-local navigation items; the state retains no controls.
Language changes and closing/reopening retain both values.
The page keeps only view instances and callback bookkeeping locally; global theme
and general settings remain in their existing shared models. Discarded views detach
their bindings and callbacks so they cannot observe or change the retained model.

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
ComboBox and checks typed state identity, retained selection and native localization,
window identity/bounds, live model-to-view updates, independent registrations of the
same page type, closing/reopening both sections, repeated reloads, input-pause
cleanup, and compatibility with stateless registration. It restores the original language preference, writes
`reload-verification.txt` beside the executable, and closes the app. Test code is
excluded from ordinary builds. The style override accommodates existing `IDE0008`
diagnostics elsewhere in the repository; it does not disable compiler errors.
