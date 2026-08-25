using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;
using Windows.UI;

namespace Gami;

public static partial class EasyWindows {

	public enum BackdropMaterial {
		Mica,
		MicaAlt,
		Acrylic,
		AcrylicThin
	}

	public sealed class ButtonOptions {
		public required Symbol Icon { get; init; }
		public required RoutedEventHandler Action { get; init; }
	}

	public record ThemeSettings {
		public BackdropMaterial backdropMaterial { get; set; }
		public SystemBackdropTheme theme { get; set; }

		public bool shouldOverride { get; set; }
		public bool isFirstTimeOverriding { get; set; }
		public Color fallbackColor { get; set; }
		public Color tintColor { get; set; }
		public float tintOpacity { get; set; }
		public float luminosityOpacity { get; set; }
	}


	public sealed class WindowOptions {
		public required Func<Window> Factory { get; init; }
		public required string Title { get; init; }
		public required SizeInt32 DefaultSize { get; init; }
		public AppWindowPresenterKind PresenterKind { get; init; } = AppWindowPresenterKind.Default;
		public ButtonOptions? Button { get; init; }
		public bool IsResizable { get; init; } = true;
		public bool IsMinimizable { get; init; } = true;
		public bool IsMaximizable { get; init; } = true;
		public PointInt32? DefaultPosition { get; init; }
	}

}
