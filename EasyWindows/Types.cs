using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using Windows.UI;

namespace Gami;

public static partial class EasyWindows {

	public enum ThemeType {
		Mica,
		Acrylic,
		//None
	}
	public record ThemeSettings {
		public ThemeType type { get; set; }
		public MicaKind micaKind { get; set; }
		public DesktopAcrylicKind acrylicKind { get; set; }
		public SystemBackdropTheme theme { get; set; }

		public bool shouldOverride { get; set; }
		public bool isFirstTimeOverriding { get; set; }
		public Color fallbackColor { get; set; }
		public Color tintColor { get; set; }
		public float tintOpacity { get; set; }
		public float luminosityOpacity { get; set; }
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
		//TODO: implement this
		public SizeInt32 defaultPosition;

	}

}