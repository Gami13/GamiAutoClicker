# TODO

## Settings and persistence

- [ ] Add an explicit **Save** action to the settings navigation footer. Keep appearance/settings changes live in memory, but write them to disk only when Save is clicked. Define the behavior for closing the settings window with unsaved changes before implementation.
- [ ] Add settings schema/versioning, corruption recovery, diagnostics, and a more complete persistence abstraction before release.

## Window management

- [ ] Make window creation/registration transactional: construct and fully initialize a `WindowController` before publishing it in `WindowControllers`, or remove the entry if initialization fails. Add focused failure-path tests.

## Platform and release validation

- [ ] Validate trimmed and NativeAOT publish configurations independently; add required annotations/configuration or disable unsupported modes.
- [ ] Investigate compile-time generation of localized language display names instead of maintaining language-name entries manually.

## Click engine

- [ ] Centralize clicking status/state transitions so blocked or failed clicking is represented consistently by the engine and UI.

## Localization and accessibility

- [ ] Complete localization review, including hard-coded language names and app/window titles.
- [ ] Complete keyboard, Narrator, high-contrast, text-scaling, and RTL validation before release.
