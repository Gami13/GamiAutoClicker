using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;
using WinRT.Interop;

namespace Gami;

public static partial class EasyWindows {
	internal static bool IsWindowOpen(object key) => Windows.ContainsKey(key);
	internal static AppWindow GetAppWindow(object key) {
		var window = Windows.TryGetValue(key, out var c) ? c.Window : null;
		if (window == null) throw new InvalidOperationException($"Window {key} not registered.");
		return AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
	}

	private static void ApplyToAllWindows(Action<WindowController> action) {
		foreach (var c in Windows.Values) action(c);
	}
	private static void ApplyToAllAdapters(Action<IBackdropAdapter> action) {
		if (!Theme.shouldOverride) return;

		ApplyToAllWindows(window => {
			if (window.Adapter is { } adapter) action(adapter);
		});
	}
	private static void CaptureSystemBackdropDefaults(bool force = false) {
		if (HasCapturedSystemBackdropDefaults && !force) return;

		foreach (var window in Windows.Values) {
			if (window.TryCaptureBackdropDefaults()) {
				HasCapturedSystemBackdropDefaults = true;
				return;
			}
		}
	}
	public static void CreateWindow(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions for {key} not found.");
		}
		if (Windows.TryGetValue(key, out var controller)) {
			controller.Window.Activate();
			GetAppWindow(key).MoveInZOrderAtTop();
			return;
		}

		var window = options.Factory();
		_ = WindowController.Register(window, key);
		CaptureSystemBackdropDefaults();

		window.Activate();
	}
	public static void SetOverrides(bool state) {
		if (state && !HasCustomBackdropValues) {
			CaptureSystemBackdropDefaults(force: true);
		}

		Theme.shouldOverride = state;
		ApplyToAllWindows(theme => theme.SetOverrides());
	}
	public static void SetBackdropMaterial(BackdropMaterial material) {
		Theme.backdropMaterial = material;
		ApplyToAllWindows(theme => theme.CreateAdapter());

	}
	public static void SetTheme(SystemBackdropTheme theme) {
		Theme.theme = theme;
		ApplyToAllWindows(theme => theme.SetTheme());
	}
	public static void SetFallbackColor(Color color) {
		Theme.fallbackColor = color;
		HasCustomBackdropValues = true;
		ApplyToAllAdapters(adapter => adapter.FallbackColor = color);
	}
	public static void SetTintColor(Color color) {
		Theme.tintColor = color;
		HasCustomBackdropValues = true;
		ApplyToAllAdapters(adapter => adapter.TintColor = color);
	}
	public static void SetTintOpacity(float opacity) {
		Theme.tintOpacity = opacity;
		HasCustomBackdropValues = true;
		ApplyToAllAdapters(adapter => adapter.TintOpacity = opacity);
	}
	public static void SetLuminosityOpacity(float opacity) {
		Theme.luminosityOpacity = opacity;
		HasCustomBackdropValues = true;
		ApplyToAllAdapters(adapter => adapter.LuminosityOpacity = opacity);
	}
	public static void RestoreThemeDefaults() {
		Theme.backdropMaterial = BackdropMaterial.Acrylic;
		Theme.theme = SystemBackdropTheme.Default;
		Theme.shouldOverride = false;
		HasCustomBackdropValues = false;

		ApplyToAllWindows(window => {
			window.SetTheme();
			window.CreateAdapter();
		});
		CaptureSystemBackdropDefaults(force: true);
	}
	internal static WindowOptions GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions for {key} not found.", nameof(key));
		}
		return options;
	}
}
