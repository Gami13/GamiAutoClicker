using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Reactor;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace GamiAutoClicker;

// Reuses EasyWindows' theme model and backdrop adapters for Reactor-owned windows.
internal sealed class ReactorAppearance
{
    private readonly Dictionary<ReactorWindow, WindowTheme> _windows = new();

    internal ReactorAppearance() => EasyWindows.ThemeChanged += (_, _) => Refresh();

    internal void Attach(ReactorWindow window)
    {
        if (_windows.ContainsKey(window)) return;
        var entry = new WindowTheme(window);
        _windows.Add(window, entry);
        entry.Subscribe();
        entry.Refresh();
        if (_windows.Count != 1 || EasyWindows.Theme.ShouldOverride || entry.Adapter is not { } adapter) return;

        // Initialize the same override sliders/colors from the system controller as EasyWindows does.
        EasyWindows.SetFallbackColor(adapter.FallbackColor);
        EasyWindows.SetTintColor(adapter.TintColor);
        EasyWindows.SetTintOpacity(adapter.TintOpacity);
        EasyWindows.SetLuminosityOpacity(adapter.LuminosityOpacity);
    }

    internal void Detach(ReactorWindow window)
    {
        if (_windows.Remove(window, out WindowTheme? entry)) entry.Dispose();
    }

    private void Refresh()
    {
        foreach (WindowTheme entry in _windows.Values) entry.Refresh();
    }

    private sealed class WindowTheme(ReactorWindow window) : IDisposable
    {
        private readonly SystemBackdropConfiguration _configuration = new() { IsInputActive = true };
        private EasyWindows.BackdropMaterial? _material;
        private bool _overrides;
        private FrameworkElement? _root;
        internal IBackdropAdapter? Adapter { get; private set; }

        internal void Subscribe()
        {
            window.NativeWindow.Activated += OnActivated;
            _root = window.NativeWindow.Content as FrameworkElement;
            if (_root is not null) _root.ActualThemeChanged += OnActualThemeChanged;
        }

        internal void Refresh()
        {
            SystemBackdropTheme theme = EasyWindows.Theme.Theme;
            if (window.NativeWindow.Content is FrameworkElement content)
                content.RequestedTheme = theme switch
                {
                    SystemBackdropTheme.Dark => ElementTheme.Dark,
                    SystemBackdropTheme.Light => ElementTheme.Light,
                    _ => ElementTheme.Default
                };
            _configuration.Theme = _root?.ActualTheme switch
            {
                ElementTheme.Dark => SystemBackdropTheme.Dark,
                ElementTheme.Light => SystemBackdropTheme.Light,
                _ => theme
            };
            window.AppWindow.TitleBar.PreferredTheme = _configuration.Theme switch
            {
                SystemBackdropTheme.Dark => Microsoft.UI.Windowing.TitleBarTheme.Dark,
                SystemBackdropTheme.Light => Microsoft.UI.Windowing.TitleBarTheme.Light,
                _ => Microsoft.UI.Windowing.TitleBarTheme.UseDefaultAppMode
            };

            EasyWindows.ThemeSettings settings = EasyWindows.Theme;
            if (_material == settings.BackdropMaterial && _overrides == settings.ShouldOverride)
            {
                if (settings.ShouldOverride && Adapter is { } adapter)
                {
                    adapter.FallbackColor = settings.FallbackColor;
                    adapter.TintColor = settings.TintColor;
                    adapter.TintOpacity = settings.TintOpacity;
                    adapter.LuminosityOpacity = settings.LuminosityOpacity;
                }
                return;
            }

            _material = settings.BackdropMaterial;
            _overrides = settings.ShouldOverride;
            IBackdropAdapter? replacement = settings.BackdropMaterial switch
            {
                EasyWindows.BackdropMaterial.Mica when MicaController.IsSupported() =>
                    new MicaAdapter(window.NativeWindow, _configuration, MicaKind.Base),
                EasyWindows.BackdropMaterial.MicaAlt when MicaController.IsSupported() =>
                    new MicaAdapter(window.NativeWindow, _configuration, MicaKind.BaseAlt),
                EasyWindows.BackdropMaterial.Acrylic when DesktopAcrylicController.IsSupported() =>
                    new AcrylicAdapter(window.NativeWindow, _configuration, DesktopAcrylicKind.Base),
                EasyWindows.BackdropMaterial.AcrylicThin when DesktopAcrylicController.IsSupported() =>
                    new AcrylicAdapter(window.NativeWindow, _configuration, DesktopAcrylicKind.Thin),
                _ => null
            };
            Adapter?.Dispose();
            Adapter = replacement;
        }

        public void Dispose()
        {
            window.NativeWindow.Activated -= OnActivated;
            if (_root is not null) _root.ActualThemeChanged -= OnActualThemeChanged;
            Adapter?.Dispose();
            Adapter = null;
        }

        private void OnActivated(object sender, WindowActivatedEventArgs args) =>
            _configuration.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;

        private void OnActualThemeChanged(FrameworkElement sender, object args) => Refresh();
    }
}
