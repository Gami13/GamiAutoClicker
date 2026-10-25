# Reactor parity work — 2026-10-25

The earlier port's listed UI and library-architecture gaps have been addressed. The application now uses `GamiAutoClicker.Reactor -> GamiToolkit.Reactor -> EasyWindows.Reactor`. Its runtime project dependencies include neither of the original XAML library assemblies.

## Changes

| Area | Restored behavior |
| --- | --- |
| Shared libraries | Window registration/lifecycle, retained state, title bars and backdrops belong to EasyWindows.Reactor. Reusable settings controls and persistence belong to GamiToolkit.Reactor. Native backdrop and non-UI protobuf implementations remain shared source. |
| Main window | Original cards/margins, bottom status placement, themed status brushes, leading title-bar settings icon, physical window bounds and presenter restrictions. |
| Numeric input | Units inside the number boxes, inline spinners, original decimal precision and increments, clear-button removal, full engine precision when converting CPS. |
| Settings layout | Separate header and constrained scrolling content. Restore collapses at 470; settings cards switch columns at 590; color controls switch columns at 820, based on actual content width. |
| Color tuning | Alpha checkerboard, hex swatch preview, dropdown indicator, ring picker, native themed flyout styling and Acrylic backdrop, one-percent slider increments. |
| Navigation | Transparent/high-contrast pane resources, compact inline rail, accent Save footer that does not change the selected page, retained selected page and pane-open state. |
| Lifecycle | Theme/language updates across windows; content reload preserves native windows and engine lifetime. Closing Settings releases editing scope and discards unsaved settings. |
| Error recovery | Invalid key edits restore the displayed model value, repeated validation/save errors can reopen, and key hints refresh even while clicking is already disabled. |

## Validation

- Managed x64 Release build: zero warnings and errors.
- Native UI verification executable: **37 passing checks**, ending in `SUCCESS`.
- Verified numeric precision, native control configuration, color changes, layout at 1110px and 434px, validation recovery, hotkey hints, language/theme updates, window reuse, retained navigation, save/discard/disk reload, and content reload.
- Clicking stayed disabled. Tests write only to an isolated settings file beside their report; the application's default settings path and format are retained.
- The runtime dependency file was inspected to confirm that the original EasyWindows/GamiToolkit assemblies are absent.
- `git diff --check` passed.

The available toolchain was .NET SDK 11 RC2 targeting .NET 10, with the cached Windows SDK reference 10.0.26100.57 and Microsoft.UI.Reactor 0.1.0-preview.16 / Windows App SDK 2.4.0. The runnable preview at `GamiAutoClicker.Reactor/bin/port-preview/GamiAutoClicker.Reactor.exe` is framework-dependent and uses the installed .NET 10 runtime; it carries its Windows App Runtime dependencies.

## Practical limits

Appearance was compared through source and native layout/control checks, without screenshot comparison. Physical mouse injection, signed installation, Native AOT publishing and the other CPU architectures were not validated in this pass. Previous CPU/memory numbers precede these UI changes and were not rerun.

See [the application guide](../GamiAutoClicker.Reactor/README.md) and [the separate verification runner](../tests/GamiAutoClicker.Reactor.Verification/Program.cs). Raw local reports/build logs are under ignored `benchmarks/results/`.

## Title-bar correction after visual feedback

Restored native `CaptionTextBlockStyle` typography, the original grid column spacing, and the theme-defined button border. Native checks confirm caption font metrics match the original style and the button has a left inset of 1 at the test display scale. The expanded verification suite passes 40 checks. No screenshot comparison is claimed.
