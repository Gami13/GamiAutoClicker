using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

using Windows.Graphics;
using Windows.UI;

namespace GamiAutoClicker;

public enum ThemeType {
	Mica,
	Acrylic,
	None
}
public struct ThemeSettings {
	public ThemeType type;
	public MicaKind micaKind;
	public DesktopAcrylicKind acrylicKind;
	public SystemBackdropTheme theme;

	public bool shouldOverride;
	public bool isFirstTimeOverriding;
	public Color fallbackColor;
	public Color tintColor;
	public float tintOpacity;
	public float luminosityOpacity;
}


public struct WindowConfig {
	public Func<Window> windowConstructor;
	public AppWindowPresenterKind presenterKind;
	public string title;
	public bool hasButton;
	public Symbol buttonIcon;
	public RoutedEventHandler? buttonAction;
	public bool isResizable;
	public bool isMinimizable;
	public bool isMaximizable;
	public SizeInt32 defaultSize;
	public SizeInt32 defaultPosition;

}


public enum WindowKey {
	Main,
	Settings
}

public static class Constants {

	public static readonly Dictionary<WindowKey, WindowConfig> WindowConfigs = new() {
	{
		WindowKey.Main,
		new WindowConfig {
			windowConstructor = () => new MainWindow(),
			presenterKind = AppWindowPresenterKind.Default,
			title = "Gami's AutoClicker",
			hasButton = true,
			buttonIcon = Symbol.Setting,
			buttonAction = Utilities.OpenSettingsWindow,
			isResizable = false,
			isMinimizable = false,
			isMaximizable = false,
			defaultSize = new SizeInt32(370, 290),
			defaultPosition = new SizeInt32(100, 100)
		}
	},
	{
		WindowKey.Settings,
		new WindowConfig {
			windowConstructor = () => new SettingsWindow(),
			presenterKind = AppWindowPresenterKind.Overlapped,
			title = "Settings",
			hasButton = false,
			buttonIcon = Symbol.Delete,
			buttonAction = null,
			isResizable = true,
			isMinimizable = true,
			isMaximizable = true,
			defaultSize = new SizeInt32(400, 300),
			defaultPosition = new SizeInt32(200, 200)
		}
	}
};
}