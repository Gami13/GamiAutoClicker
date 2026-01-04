using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using System;
using Windows.UI;
using WinRT.Interop;

namespace GamiAutoClicker.WindowManager;

public static class ThemeHelper {
	public static bool IsWindowOpen(WindowKey key) => Configuration.Windows.ContainsKey(key);
	public static AppWindow GetAppWindow(WindowKey key) {
		var window = Configuration.Windows.TryGetValue(key, out var c) ? c.Window : null;
		if (window == null) throw new InvalidOperationException($"Window {key} not registered.");
		return AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
	}

	public static void ApplyToAllWindows(Action<WindowController> action) {
		foreach (var c in Configuration.Windows.Values) action(c);
	}
	public static void CreateWindow(WindowKey key) {
		if (IsWindowOpen(key)) {
			GetAppWindow(key).MoveInZOrderAtTop();
			return;
		}
		Configuration.WindowConfigs[key].windowConstructor().Activate();
	}

	public static void SetOverrides(bool state) {
		Configuration.ThemeSettings.shouldOverride = state;
		ApplyToAllWindows(theme => theme.SetOverrides());
	}
	public static void SetType(ThemeType type) {
		Configuration.ThemeSettings.type = type;
		ApplyToAllWindows(theme => theme.Backdrop?.CreateController());

	}
	public static void SetTheme(SystemBackdropTheme theme) {
		Configuration.ThemeSettings.theme = theme;
		ApplyToAllWindows(theme => theme.SetTheme());
	}
	public static void SetMicaKind(MicaKind kind) {
		Configuration.ThemeSettings.micaKind = kind;
		ApplyToAllWindows(theme => theme.Backdrop?.SetMicaKind());
	}
	public static void SetAcrylicKind(DesktopAcrylicKind kind) {
		Configuration.ThemeSettings.acrylicKind = kind;
		ApplyToAllWindows(theme => theme.Backdrop?.SetAcrylicKind());
	}
	public static void SetFallbackColor(Color color) {
		Configuration.ThemeSettings.fallbackColor = color;
		ApplyToAllWindows(theme => theme.Backdrop?.SetFallbackColor());
	}
	public static void SetTintColor(Color color) {
		Configuration.ThemeSettings.tintColor = color;
		ApplyToAllWindows(theme => theme.Backdrop?.SetTintColor());
	}
	public static void SetTintOpacity(float opacity) {
		Configuration.ThemeSettings.tintOpacity = opacity;
		ApplyToAllWindows(theme => theme.Backdrop?.SetTintOpacity());
	}
	public static void SetLuminosityOpacity(float opacity) {
		Configuration.ThemeSettings.luminosityOpacity = opacity;
		ApplyToAllWindows(theme => theme.Backdrop?.SetLuminosityOpacity());
	}
}

