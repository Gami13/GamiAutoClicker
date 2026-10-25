# EasyWindows for Reactor

Owns the windowing layer independently of application pages and persistence. The API keeps the original `Gami.EasyWindows` entry point and options models, with Reactor element factories instead of XAML content factories.

Register windows with `RegisterWindow<TKey>`, start a primary `RegisteredWindow` subclass using `GetWindowSpec`, and open secondary windows with `CreateWindow`. `WindowOptions<TState>` retains application-owned state. `GetWindow` exposes the live Reactor window; `ReloadAllWindows` replaces content while retaining native window identity and bounds. `RefreshTitles` updates localized title bars without replacing content.

`TitleBar.cs` supplies the custom title bar and non-client hit regions. `WindowController.cs` owns focus/theme events, controller cleanup, default position and close callbacks. The native backdrop adapters, flyout backdrop and dispatcher-queue helper are linked from the original library so fixes apply to both frontends. The original XAML library is not a project dependency.
