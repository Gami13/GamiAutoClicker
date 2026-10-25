# GamiToolkit for Reactor

References `EasyWindows.Reactor` and hosts reusable components under `Components/`. Pages provide text and callbacks; the toolkit handles control styling and lifecycle.

- `SettingsControls`: themed cards, compact restore buttons, unit-aware number boxes with inline spinners, compact toggles and percentage sliders.
- `ColorPickerFlyoutButton`: alpha preview, dropdown, ring picker and themed flyout.
- `ResponsiveComponent`: content-width observation that attaches after native mounting and detaches on unmount.
- `NavigationViewBehavior`: compact inline pane behavior, retained pane initialization and accent Save footer.
- `ResourceStyles`: native theme dictionaries, including high contrast.

The existing non-UI protobuf configuration and theme persistence code is linked into this assembly. Default storage paths and file formats are preserved. The original `GamiToolkit` and `EasyWindows` assemblies are not dependencies.
