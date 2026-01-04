using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using Windows.UI;

namespace GamiAutoClicker.WindowManager {
	public enum ThemeType {
		Mica,
		Acrylic,
		//None
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

}
