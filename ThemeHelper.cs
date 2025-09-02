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
		MainWindow.mainWindowTheme?.SetOverrides();
		MainWindow.settingsWindowTheme?.SetOverrides();
	}
	public static void SetType(ThemeType type) {
		MainWindow.themeSettings.type = type;
		MainWindow.mainWindowTheme?.SetType();
		MainWindow.settingsWindowTheme?.SetType();
	}
	public static void SetTheme(SystemBackdropTheme theme) {
		MainWindow.themeSettings.theme = theme;
		MainWindow.mainWindowTheme?.SetTheme();
		MainWindow.settingsWindowTheme?.SetTheme();
	}
	public static void SetMicaKind(MicaKind kind) {
		MainWindow.themeSettings.micaKind = kind;
		MainWindow.mainWindowTheme?.SetMicaKind();
		MainWindow.settingsWindowTheme?.SetMicaKind();
	}
	public static void SetAcrylicKind(DesktopAcrylicKind kind) {
		MainWindow.themeSettings.acrylicKind = kind;
		MainWindow.mainWindowTheme?.SetAcrylicKind();
		MainWindow.settingsWindowTheme?.SetAcrylicKind();
	}
	public static void SetFallbackColor(Color color) {
		MainWindow.themeSettings.fallbackColor = color;
		MainWindow.mainWindowTheme?.SetFallbackColor();
		MainWindow.settingsWindowTheme?.SetFallbackColor();
	}
	public static void SetTintColor(Color color) {
		MainWindow.themeSettings.tintColor = color;
		MainWindow.mainWindowTheme?.SetTintColor();
		MainWindow.settingsWindowTheme?.SetTintColor();
	}
	public static void SetTintOpacity(float opacity) {
		MainWindow.themeSettings.tintOpacity = opacity;
		MainWindow.mainWindowTheme?.SetTintOpacity();
		MainWindow.settingsWindowTheme?.SetTintOpacity();
	}
	public static void SetLuminosityOpacity(float opacity) {
		MainWindow.themeSettings.luminosityOpacity = opacity;
		MainWindow.mainWindowTheme?.SetLuminosityOpacity();
		MainWindow.settingsWindowTheme?.SetLuminosityOpacity();
	}

}

