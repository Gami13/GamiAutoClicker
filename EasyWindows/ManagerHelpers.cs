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

		window.Activate();
	}
	public static void SetOverrides(bool state) {
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
		ApplyToAllAdapters(adapter => adapter.FallbackColor = color);
	}
	public static void SetTintColor(Color color) {
		Theme.tintColor = color;
		ApplyToAllAdapters(adapter => adapter.TintColor = color);
	}
	public static void SetTintOpacity(float opacity) {
		Theme.tintOpacity = opacity;
		ApplyToAllAdapters(adapter => adapter.TintOpacity = opacity);
	}
	public static void SetLuminosityOpacity(float opacity) {
		Theme.luminosityOpacity = opacity;
		ApplyToAllAdapters(adapter => adapter.LuminosityOpacity = opacity);
	}
	internal static WindowOptions GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions for {key} not found.", nameof(key));
		}
		return options;
	}
}
