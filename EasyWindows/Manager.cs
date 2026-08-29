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
		if (window == null) throw new InvalidOperationException($"Window {key} not registered.");
		return window.AppWindow;
	}

	private static void ApplyToAllWindows(Action<WindowController> action) {
		foreach (var controller in Windows.Values) action(controller);
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

	public static void SetFallbackColor(Color color) => UpdateCustomSetting(
		(theme, value) => theme.fallbackColor = value,
		(adapter, value) => adapter.FallbackColor = value,
		color);

	public static void SetTintColor(Color color) => UpdateCustomSetting(
		(theme, value) => theme.tintColor = value,
		(adapter, value) => adapter.TintColor = value,
		color);

	public static void SetTintOpacity(float opacity) => UpdateCustomSetting(
		(theme, value) => theme.tintOpacity = value,
		(adapter, value) => adapter.TintOpacity = value,
		opacity);

	public static void SetLuminosityOpacity(float opacity) => UpdateCustomSetting(
		(theme, value) => theme.luminosityOpacity = value,
		(adapter, value) => adapter.LuminosityOpacity = value,
		opacity);

	private static void UpdateCustomSetting<T>(
		Action<ThemeSettings, T> updateTheme,
		Action<IBackdropAdapter, T> updateAdapter,
		T value) {
		updateTheme(Theme, value);
		HasCustomBackdropValues = true;
		ApplyToAllAdapters(adapter => updateAdapter(adapter, value));
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

	internal static WindowOptions GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out var options)) {
			throw new ArgumentException($"WindowOptions for {key} not found.", nameof(key));
		}
		return options;
	}
}
