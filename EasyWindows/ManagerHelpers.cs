using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using Windows.UI;
using WinRT.Interop;

namespace Gami;

public static partial class EasyWindows {
	public static partial class Manager {
		public static bool IsWindowOpen(object key) => Windows.ContainsKey(key);
		public static AppWindow GetAppWindow(object key) {
			var window = Windows.TryGetValue(key, out var c) ? c.Window : null;
			if (window == null) throw new InvalidOperationException($"Window {key} not registered.");
			return AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
		}

		public static void ApplyToAllWindows(Action<WindowController> action) {
			foreach (var c in Windows.Values) action(c);
		}
		public static void CreateWindow(object key) {
			if (!WindowConfigs.TryGetValue(key, out var config)) {
				throw new ArgumentException($"WindowConfig for {key} not found.");
			}
			if (IsWindowOpen(key)) {
				GetAppWindow(key).MoveInZOrderAtTop();
				return;
			}

			var window = config.windowConstructor();
			InitializeWindow(window, key);

			window.Activate();
		}
		public static void InitializeWindow(Window window, object key) {
			if (window.Content is UIElement originalContent) {
				var rootGrid = new Grid();
				rootGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
				rootGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

				window.Content = null;
				if (originalContent is FrameworkElement fe) {
					Grid.SetRow(fe, 1);
					rootGrid.Children.Add(fe);
				}

				window.Content = rootGrid;
			}

			WindowController.Register(window, key);
		}
		public static void SetOverrides(bool state) {
			ThemeSettings.shouldOverride = state;
			ApplyToAllWindows(theme => theme.SetOverrides());
		}
		public static void SetType(ThemeType type) {
			ThemeSettings.type = type;
			ApplyToAllWindows(theme => theme.Backdrop?.CreateController());

		}
		public static void SetTheme(SystemBackdropTheme theme) {
			ThemeSettings.theme = theme;
			ApplyToAllWindows(theme => theme.SetTheme());
		}
		public static void SetMicaKind(MicaKind kind) {
			ThemeSettings.micaKind = kind;
			ApplyToAllWindows(theme => theme.Backdrop?.SetMicaKind());
		}
		public static void SetAcrylicKind(DesktopAcrylicKind kind) {
			ThemeSettings.acrylicKind = kind;
			ApplyToAllWindows(theme => theme.Backdrop?.SetAcrylicKind());
		}
		public static void SetFallbackColor(Color color) {
			ThemeSettings.fallbackColor = color;
			ApplyToAllWindows(theme => theme.Backdrop?.SetFallbackColor());
		}
		public static void SetTintColor(Color color) {
			ThemeSettings.tintColor = color;
			ApplyToAllWindows(theme => theme.Backdrop?.SetTintColor());
		}
		public static void SetTintOpacity(float opacity) {
			ThemeSettings.tintOpacity = opacity;
			ApplyToAllWindows(theme => theme.Backdrop?.SetTintOpacity());
		}
		public static void SetLuminosityOpacity(float opacity) {
			ThemeSettings.luminosityOpacity = opacity;
			ApplyToAllWindows(theme => theme.Backdrop?.SetLuminosityOpacity());
		}
	}

}