using Microsoft.UI.Composition.SystemBackdrops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GamiAutoClicker;

static class ThemeHelper {
	private static void ApplyToAllWindows(Action<WindowController> action) {
		foreach (var controller in MainWindow.windowThemes.Values)
			action(controller);
	}
	public static void SetOverrides(bool state) {
		MainWindow.themeSettings.shouldOverride = state;
		ApplyToAllWindows(theme => theme.SetOverrides());
	}
	public static void SetType(ThemeType type) {
		MainWindow.themeSettings.type = type;
		ApplyToAllWindows(theme => theme.SetType());

	}
	public static void SetTheme(SystemBackdropTheme theme) {
		MainWindow.themeSettings.theme = theme;
		ApplyToAllWindows(theme => theme.SetTheme());
	}
	public static void SetMicaKind(MicaKind kind) {
		MainWindow.themeSettings.micaKind = kind;
		ApplyToAllWindows(theme => theme.SetMicaKind());
	}
	public static void SetAcrylicKind(DesktopAcrylicKind kind) {
		MainWindow.themeSettings.acrylicKind = kind;
		ApplyToAllWindows(theme => theme.SetAcrylicKind());
	}
	public static void SetFallbackColor(Color color) {
		MainWindow.themeSettings.fallbackColor = color;
		ApplyToAllWindows(theme => theme.SetFallbackColor());
	}
	public static void SetTintColor(Color color) {
		MainWindow.themeSettings.tintColor = color;
		ApplyToAllWindows(theme => theme.SetTintColor());
	}
	public static void SetTintOpacity(float opacity) {
		MainWindow.themeSettings.tintOpacity = opacity;
		ApplyToAllWindows(theme => theme.SetTintOpacity());
	}
	public static void SetLuminosityOpacity(float opacity) {
		MainWindow.themeSettings.luminosityOpacity = opacity;
		ApplyToAllWindows(theme => theme.SetLuminosityOpacity());
	}

}

