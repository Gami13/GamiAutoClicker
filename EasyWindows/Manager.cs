using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Windows.UI;

namespace Gami;

public static partial class EasyWindows {
	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static ThemeSettings Theme { get; } = new() {
		backdropMaterial = BackdropMaterial.Acrylic,
		theme = SystemBackdropTheme.Default,
		shouldOverride = false,
		fallbackColor = Colors.White,
		tintColor = Colors.White,
		tintOpacity = 0.0f,
		luminosityOpacity = 0.0f
	};

	private static Dictionary<object, WindowController> Windows { get; } = new();
	private static Dictionary<object, WindowOptions> WindowConfigs { get; } = new();
	private static bool HasCapturedSystemBackdropDefaults { get; set; }
	private static bool HasCustomBackdropValues { get; set; }

	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static event EventHandler? ThemeChanged;

	public static void RegisterWindow(object key, WindowOptions options) {
		ArgumentNullException.ThrowIfNull(options);

		if (!WindowConfigs.TryAdd(key, options)) {
			throw new ArgumentException($"Window {key} is already registered.", nameof(key));
		}
	}

	internal static bool IsWindowOpen(object key) => Windows.ContainsKey(key);

	internal static AppWindow GetAppWindow(object key) {
		var window = Windows.TryGetValue(key, out var controller) ? controller.Window : null;
		if (window == null) throw new InvalidOperationException($"Window {key} not created.");
		return window.AppWindow;
	}
	internal static WindowOptions GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions {key} not registered.", nameof(key));
		}
		return options;
	}

	private static void ApplyToAllWindows(Action<WindowController> action) {
		foreach (var controller in Windows.Values) action(controller);
	}

	private static void UpdateAdapter(
		WindowController window,
		Action<IBackdropAdapter> updateAdapter) {
		if (Theme.shouldOverride && window.Adapter is { } adapter) {
			updateAdapter(adapter);
		}
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

	private static void NotifyThemeChanged() => ThemeChanged?.Invoke(null, EventArgs.Empty);

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
		NotifyThemeChanged();
	}

	public static void SetBackdropMaterial(BackdropMaterial material) {
		Theme.backdropMaterial = material;
		ApplyToAllWindows(theme => theme.CreateAdapter());
		NotifyThemeChanged();
	}

	public static void SetTheme(SystemBackdropTheme theme) {
		Theme.theme = theme;
		ApplyToAllWindows(window => window.SetTheme());
		NotifyThemeChanged();
	}

	public static void SetFallbackColor(Color color) {
		Theme.fallbackColor = color;
		HasCustomBackdropValues = true;
		ApplyToAllWindows(window => UpdateAdapter(window, adapter => adapter.FallbackColor = color));
		NotifyThemeChanged();
	}

	public static void SetTintColor(Color color) {
		Theme.tintColor = color;
		HasCustomBackdropValues = true;
		ApplyToAllWindows(window => UpdateAdapter(window, adapter => adapter.TintColor = color));
		NotifyThemeChanged();
	}

	public static void SetTintOpacity(float opacity) {
		Theme.tintOpacity = opacity;
		HasCustomBackdropValues = true;
		ApplyToAllWindows(window => UpdateAdapter(window, adapter => adapter.TintOpacity = opacity));
		NotifyThemeChanged();
	}

	public static void SetLuminosityOpacity(float opacity) {
		Theme.luminosityOpacity = opacity;
		HasCustomBackdropValues = true;
		ApplyToAllWindows(window => UpdateAdapter(window, adapter => adapter.LuminosityOpacity = opacity));
		NotifyThemeChanged();
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
		NotifyThemeChanged();
	}
}
