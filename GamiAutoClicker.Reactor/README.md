# Reactor port

The application is split into the same three responsibilities as the original:

| Project | Responsibility |
| --- | --- |
| `EasyWindows.Reactor` | Typed window registration, retained registration state, native window lifecycle, custom title bars, theme model, Mica/Acrylic window and flyout backdrops. |
| `GamiToolkit.Reactor` | Protobuf persistence plus reusable settings cards, unit number boxes, color flyouts, opacity sliders, restore buttons and responsive/navigation helpers. |
| `GamiAutoClicker.Reactor` | Clicker controls, settings navigation/pages, localization and application startup. |

The Reactor dependency chain contains neither the original `EasyWindows` nor the original `GamiToolkit` assembly. Backend code that does not need a UI rewrite is shared as linked source: the click engine, general settings, settings snapshot/serialization, backdrop adapters and English/Polish translations. Shared persistence keeps its original format and default paths.

## Application layout

- `Core/AppState.cs`: startup, window registrations and the application-wide click engine lifetime. Content reloads do not restart the engine.
- `Windows/MainView.cs`: timing, button/hold settings and status. Units, precision, step sizes, cards, bottom status placement and theme brushes follow the original UI.
- `Windows/SettingsView.cs`: retained navigation state, Save and hotkey editing scope. A real settings-window close discards unsaved changes; content reloads do not.
- `Pages/GeneralPage.cs`: key bindings and language. Cards use two columns at a content width of 590, and Restore collapses to an icon below 470.
- `Pages/AppearancePage.cs`: material/theme and color tuning. Advanced settings move to the right column at 590; color controls use two columns at 820.
- `Localization.cs`: reads the original `.resw` translations embedded in this assembly.

The shared color control restores the alpha checkerboard, hex preview, dropdown affordance, ring picker and themed Acrylic flyout. The shared window layer renders the original 32-pixel title bar with its leading settings icon and interactive non-client region. The navigation rail uses compact inline behavior and a non-selecting accent Save footer item, including native high-contrast pane resources.

## Build and run

With the .NET 10 / Windows App SDK toolchain and NuGet access:

```powershell
dotnet build .\GamiAutoClicker.Reactor\GamiAutoClicker.Reactor.csproj -c Release -p:Platform=x64
dotnet run --project .\GamiAutoClicker.Reactor\GamiAutoClicker.Reactor.csproj -c Release -p:Platform=x64
```

Use `-p:Platform=ARM64` for Snapdragon or `-p:Platform=x86` for 32-bit builds. The default deployment is unpackaged and self-contained. Publish profiles retain the separate Reactor MSIX identity. Signing and Native AOT validation are separate deployment checks; a managed desktop run does not establish either.

## Verification

The separate `tests/GamiAutoClicker.Reactor.Verification` executable drives native control properties on the UI thread. It checks numeric precision, color edits, responsive placement, hotkey editing scopes, theme/language updates, window reuse, navigation retention, content reload and save/discard/reload behavior. It uses an isolated settings file beside its report and leaves clicking disabled.

```powershell
New-Item -ItemType Directory -Force .\benchmarks\results | Out-Null
$env:GAMI_VERIFICATION_REPORT = Join-Path $PWD 'benchmarks\results\reactor-parity-verification.txt'
dotnet run --project .\tests\GamiAutoClicker.Reactor.Verification -c Release -p:Platform=x64 -p:SelfContained=false
Get-Content $env:GAMI_VERIFICATION_REPORT
```

These checks validate code and native UI behavior. They do not replace a screenshot comparison, physical mouse-injection test or installation test. Benchmarks remain under `benchmarks/`; the Reactor idle-poll setting remains 25 ms.
