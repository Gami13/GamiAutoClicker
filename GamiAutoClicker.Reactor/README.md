# Reactor port

`GamiAutoClicker.Reactor` is the side-by-side Microsoft.UI.Reactor version of the app. The original `GamiAutoClicker` project remains the baseline. Both use the same click engine, general settings, protobuf settings snapshot, theme model, backdrop adapters, and English/Polish translations. The Reactor project owns its window and page components; its `ReactorAppearance` service applies the existing theme to Reactor windows.

## Layout

- `Core/AppState.cs`: startup, shared click engine, settings-window state, window opening.
- `Core/ReactorAppearance.cs`: backdrop and theme integration for Reactor windows.
- `Windows/MainView.cs`: clicking controls and engine lifetime.
- `Windows/SettingsView.cs`: settings navigation, Save, and discard on close.
- `Pages/GeneralPage.cs` and `Pages/AppearancePage.cs`: the two settings sections.
- `Localization.cs`: reads the original `.resw` translations embedded in this assembly, so unpackaged and MSIX builds use the same language data.

Settings changes apply immediately. Save writes the existing `%LOCALAPPDATA%\GamiAutoClicker\settings.bin` format; closing Settings restores its last saved snapshot. The settings window remembers its selected section and pane state when reopened. The main window retains F8 toggle and hold-to-click behavior through the shared `ClickEngine`.

## Build and run

With NuGet access and the .NET 10 / Windows App SDK toolchain:

```powershell
dotnet build .\GamiAutoClicker.Reactor\GamiAutoClicker.Reactor.csproj -c Release -p:Platform=x64
dotnet run --project .\GamiAutoClicker.Reactor\GamiAutoClicker.Reactor.csproj -c Release -p:Platform=x64
```

Use `-p:Platform=ARM64` for a native Snapdragon build, or `-p:Platform=x86` for 32-bit Windows. The default project is unpackaged and carries the Windows App Runtime in its output folder. The `win-x64`, `win-arm64`, and `win-x86` publish profiles opt into MSIX with a separate `GamiAutoClicker.Reactor` package identity, allowing installation beside the original. A signed installable package requires a matching certificate. x64 and ARM64 profiles request Native AOT; x86 uses managed code. The unsigned, managed x64 MSIX build has been verified, but Native AOT publishing could not complete in this sandbox because MSBuild could not start its x64 task host.

The Reactor dependency is pinned to `0.1.0-preview.16`. Benchmarks are in `benchmarks/` at the repository root. The port is still a preview until its packaged/AOT output and mouse injection are validated on a normal desktop outside the sandbox.
