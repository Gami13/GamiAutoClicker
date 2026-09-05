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

	private static Dictionary<object, WindowController> WindowControllers { get; } = new();
	private static Dictionary<object, WindowOptions> WindowConfigs { get; } = new();
	private static BackdropValueSource BackdropValues { get; set; }

	private enum BackdropValueSource {
		Uninitialized,
		SystemDefaults,
		Custom
	}

	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static event EventHandler? ThemeChanged;

	public static void RegisterWindow(object key, WindowOptions options) {
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(options);

		if (!WindowConfigs.TryAdd(key, options)) {
			throw new ArgumentException($"Window {key} is already registered.", nameof(key));
		}
	}

	internal static bool IsWindowOpen(object key) => WindowControllers.ContainsKey(key);

	internal static AppWindow GetAppWindow(object key) {
		var window = WindowControllers.TryGetValue(key, out var controller) ? controller.Window : null;
		if (window == null) throw new InvalidOperationException($"Window {key} not created.");
		return window.AppWindow;
	}
	internal static WindowOptions GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions {key} not registered.", nameof(key));
		}
		return options;
	}

	private static void ApplyToAllWindowControllers(Action<WindowController> action) {
		foreach (var controller in WindowControllers.Values) action(controller);
	}

	private static void UpdateAdapter(
		WindowController window,
		Action<IBackdropAdapter> updateAdapter) {
		if (Theme.shouldOverride && window.Adapter is { } adapter) {
			updateAdapter(adapter);
		}
	}

	private static void CaptureSystemBackdropDefaults(bool force = false) {
		if (BackdropValues != BackdropValueSource.Uninitialized && !force) return;

		foreach (var window in WindowControllers.Values) {
			if (window.TryCaptureBackdropDefaults()) {
				BackdropValues = BackdropValueSource.SystemDefaults;
				break;
			}
		}
	}

	private static void NotifyThemeChanged() => ThemeChanged?.Invoke(null, EventArgs.Empty);

	private static void EnsureOpacityInRange(float opacity, string parameterName) {
		if (opacity is < 0.0f or > 1.0f) {
			throw new ArgumentOutOfRangeException(parameterName, opacity, "Opacity must be between 0 and 1.");
		}
	}

	public static void CreateWindow(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions for {key} not found.");
		}
		if (WindowControllers.TryGetValue(key, out var controller)) {
			controller.Window.Activate();
			controller.Window.AppWindow.MoveInZOrderAtTop();
			return;
		}

		var window = options.Factory();
		_ = WindowController.Register(window, key);
		CaptureSystemBackdropDefaults();

		window.Activate();
	}

	public static void SetOverrides(bool state) {
		if (state == Theme.shouldOverride) {
			return;
		}

		if (state && BackdropValues != BackdropValueSource.Custom) {
			CaptureSystemBackdropDefaults(force: true);
		}

		Theme.shouldOverride = state;
		ApplyToAllWindowControllers(theme => theme.SetOverrides());
		NotifyThemeChanged();
	}

	public static void SetBackdropMaterial(BackdropMaterial material) {
		if (!Enum.IsDefined(material)) {
			throw new ArgumentOutOfRangeException(nameof(material));
		}

		if (material == Theme.backdropMaterial) {
			return;
		}

		Theme.backdropMaterial = material;
		ApplyToAllWindowControllers(window => window.CreateAdapter());
		NotifyThemeChanged();
	}

	public static void ApplyTheme(SystemBackdropTheme theme) {
		if (!Enum.IsDefined(theme)) {
			throw new ArgumentOutOfRangeException(nameof(theme));
		}

		if (theme == Theme.theme) {
			return;
		}

		Theme.theme = theme;
		ApplyToAllWindowControllers(window => window.SetTheme());
		NotifyThemeChanged();
	}

	public static void SetFallbackColor(Color color) {
		if (color == Theme.fallbackColor) {
			return;
		}

		Theme.fallbackColor = color;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.FallbackColor = color));
		NotifyThemeChanged();
	}

	public static void SetTintColor(Color color) {
		if (color == Theme.tintColor) {
			return;
		}

		Theme.tintColor = color;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.TintColor = color)); NotifyThemeChanged();
	}

	public static void SetTintOpacity(float opacity) {
		EnsureOpacityInRange(opacity, nameof(opacity));
		if (opacity == Theme.tintOpacity) {
			return;
		}

		Theme.tintOpacity = opacity;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.TintOpacity = opacity));
		NotifyThemeChanged();
	}

	public static void SetLuminosityOpacity(float opacity) {
		EnsureOpacityInRange(opacity, nameof(opacity));
		if (opacity == Theme.luminosityOpacity) {
			return;
		}

		Theme.luminosityOpacity = opacity;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.LuminosityOpacity = opacity));
		NotifyThemeChanged();
	}

	public static void RestoreThemeDefaults() {
		Theme.backdropMaterial = BackdropMaterial.Acrylic;
		Theme.theme = SystemBackdropTheme.Default;
		Theme.shouldOverride = false;
		// Theme.fallbackColor = new Color { A = byte.MaxValue, R = byte.MaxValue, G = byte.MaxValue, B = byte.MaxValue };
		// Theme.tintColor = new Color { A = byte.MaxValue, R = byte.MaxValue, G = byte.MaxValue, B = byte.MaxValue };
		// Theme.tintOpacity = 0.0f;
		// Theme.luminosityOpacity = 0.0f;
		BackdropValues = BackdropValueSource.Uninitialized;

		ApplyToAllWindowControllers(window => {
			window.SetTheme();
			window.CreateAdapter();
		});
		CaptureSystemBackdropDefaults(force: true);

		NotifyThemeChanged();
	}
}
