using Microsoft.UI.Composition.SystemBackdrops;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace GamiAutoClicker;

static class ThemeHelper {
	public static void SetOverrides(bool state) {
		MainWindow.themeSettings.shouldOverride = state;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetOverrides();
		}
	}
	public static void SetType(ThemeType type) {
		MainWindow.themeSettings.type = type;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetType();
		}

	}
	public static void SetTheme(SystemBackdropTheme theme) {
		MainWindow.themeSettings.theme = theme;
		foreach (var t in MainWindow.windowThemes.Values) {
			t.SetTheme();
		}
	}
	public static void SetMicaKind(MicaKind kind) {
		MainWindow.themeSettings.micaKind = kind;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetMicaKind();
		}
	}
	public static void SetAcrylicKind(DesktopAcrylicKind kind) {
		MainWindow.themeSettings.acrylicKind = kind;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetAcrylicKind();
		}
	}
	public static void SetFallbackColor(Color color) {
		MainWindow.themeSettings.fallbackColor = color;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetFallbackColor();
		}
	}
	public static void SetTintColor(Color color) {
		MainWindow.themeSettings.tintColor = color;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetTintColor();
		}
	}
	public static void SetTintOpacity(float opacity) {
		MainWindow.themeSettings.tintOpacity = opacity;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetTintOpacity();
		}

	}
	public static void SetLuminosityOpacity(float opacity) {
		MainWindow.themeSettings.luminosityOpacity = opacity;
		foreach (var theme in MainWindow.windowThemes.Values) {
			theme.SetLuminosityOpacity();
		}
	}

}

