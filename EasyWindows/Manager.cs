using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using Windows.UI;
using Microsoft.UI.Xaml;

namespace Gami;

public static partial class EasyWindows {
	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static ThemeSettings Theme { get; } = new() {
		BackdropMaterial = BackdropMaterial.Acrylic,
		Theme = SystemBackdropTheme.Default,
		ShouldOverride = false,
		FallbackColor = Colors.White,
		TintColor = Colors.White,
		TintOpacity = 0.0f,
		LuminosityOpacity = 0.0f
	};

	private static Dictionary<object, WindowController> WindowControllers { get; } = new();
	private static Dictionary<object, WindowOptionsBase> WindowConfigs { get; } = new();
	/// <summary>Read-only view of all registrations, including windows that are currently closed.</summary>
	public static IReadOnlyDictionary<object, WindowOptionsBase> Windows { get; } =
		new ReadOnlyDictionary<object, WindowOptionsBase>(WindowConfigs);
	private static BackdropValueSource BackdropValues { get; set; }

	private enum BackdropValueSource {
		Uninitialized,
		SystemDefaults,
		Custom
	}

	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static event EventHandler? ThemeChanged;

	public static void RegisterWindow(object key, WindowOptionsBase options) {
		ArgumentNullException.ThrowIfNull(key);
		ArgumentNullException.ThrowIfNull(options);

		if (!WindowConfigs.TryAdd(key, options)) {
			throw new ArgumentException($"Window {key} is already registered.", nameof(key));
		}
	}

	internal static bool IsWindowOpen(object key) => WindowControllers.ContainsKey(key);

	private static bool _isReloading;

	/// <summary>
	/// Recreates content in every open window using its registered factory.
	/// Call on the windows' UI thread. Window identity and bounds are retained;
	/// content initializes its own state. Closed windows remain closed.
	/// </summary>
	public static void ReloadAllWindows() {
		if (_isReloading) throw new InvalidOperationException("Window reload is already in progress.");
		WindowController[] controllers = WindowControllers.Values.ToArray();
		foreach (WindowController controller in controllers) controller.ValidateReload();
		_isReloading = true;
		try {
			foreach (WindowController controller in controllers) controller.ReloadContent();
		}
		finally { _isReloading = false; }
	}

	internal static AppWindow GetAppWindow(object key) {
		Window? window = WindowControllers.TryGetValue(key, out WindowController? controller) ? controller.Window : null;
		if (window == null) throw new InvalidOperationException($"Window {key} not created.");
		return window.AppWindow;
	}
	internal static WindowOptionsBase GetWindowOptions(object key) {
		if (!WindowConfigs.TryGetValue(key, out WindowOptionsBase? options)) {
			throw new ArgumentException($"Window options {key} not registered.", nameof(key));
		}
		return options;
	}

	private static void ApplyToAllWindowControllers(Action<WindowController> action) {
		foreach (WindowController controller in WindowControllers.Values) action(controller);
	}

	private static void UpdateAdapter(
		WindowController window,
		Action<IBackdropAdapter> updateAdapter) {
		if (Theme.ShouldOverride && window.Adapter is { } adapter) {
			updateAdapter(adapter);
		}
	}

	private static void CaptureSystemBackdropDefaults(bool force = false) {
		if (BackdropValues != BackdropValueSource.Uninitialized && !force) return;

		foreach (WindowController window in WindowControllers.Values) {
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
		if (!WindowConfigs.TryGetValue(key, out WindowOptionsBase? options)) {
			throw new ArgumentException($"Window options for {key} not found.");
		}
		if (WindowControllers.TryGetValue(key, out WindowController? controller)) {
			controller.Window.Activate();
			controller.Window.AppWindow.MoveInZOrderAtTop();
			return;
		}

		Window window = new();
		_ = WindowController.Register(window, key);
		CaptureSystemBackdropDefaults();

		window.Activate();
	}

	public static void SetOverrides(bool state) {
		if (state == Theme.ShouldOverride) {
			return;
		}

		if (state && BackdropValues != BackdropValueSource.Custom) {
			CaptureSystemBackdropDefaults(force: true);
		}

		Theme.ShouldOverride = state;
		ApplyToAllWindowControllers(window => window.CreateAdapter());
		NotifyThemeChanged();
	}

	public static void SetBackdropMaterial(BackdropMaterial material) {
		if (!Enum.IsDefined(material)) {
			throw new ArgumentOutOfRangeException(nameof(material));
		}

		if (material == Theme.BackdropMaterial) {
			return;
		}

		Theme.BackdropMaterial = material;
		ApplyToAllWindowControllers(window => window.CreateAdapter());
		NotifyThemeChanged();
	}

	public static void ApplyTheme(SystemBackdropTheme theme) {
		if (!Enum.IsDefined(theme)) {
			throw new ArgumentOutOfRangeException(nameof(theme));
		}

		if (theme == Theme.Theme) {
			return;
		}

		Theme.Theme = theme;
		ApplyToAllWindowControllers(window => window.SetTheme());
		NotifyThemeChanged();
	}

	public static void SetFallbackColor(Color color) {
		if (color == Theme.FallbackColor) {
			return;
		}

		Theme.FallbackColor = color;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.FallbackColor = color));
		NotifyThemeChanged();
	}

	public static void SetTintColor(Color color) {
		if (color == Theme.TintColor) {
			return;
		}

		Theme.TintColor = color;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.TintColor = color)); NotifyThemeChanged();
	}

	public static void SetTintOpacity(float opacity) {
		EnsureOpacityInRange(opacity, nameof(opacity));
		if (opacity == Theme.TintOpacity) {
			return;
		}

		Theme.TintOpacity = opacity;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.TintOpacity = opacity));
		NotifyThemeChanged();
	}

	public static void SetLuminosityOpacity(float opacity) {
		EnsureOpacityInRange(opacity, nameof(opacity));
		if (opacity == Theme.LuminosityOpacity) {
			return;
		}

		Theme.LuminosityOpacity = opacity;
		BackdropValues = BackdropValueSource.Custom;
		ApplyToAllWindowControllers(window => UpdateAdapter(window, adapter => adapter.LuminosityOpacity = opacity));
		NotifyThemeChanged();
	}

	public static void RestoreThemeDefaults() {
		Theme.BackdropMaterial = BackdropMaterial.Acrylic;
		Theme.Theme = SystemBackdropTheme.Default;
		Theme.ShouldOverride = false;
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
