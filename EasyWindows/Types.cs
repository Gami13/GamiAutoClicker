using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
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

	[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Consumers intentionally use the cohesive EasyWindows.ButtonOptions API.")]
	public sealed class ButtonOptions {
		public required Symbol Icon { get; init; }
		public required RoutedEventHandler Action { get; init; }
		public Func<string>? AccessibleNameProvider { get; init; }
	}

	[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Consumers intentionally use the cohesive EasyWindows.ThemeSettings API.")]
	public record ThemeSettings {
		public BackdropMaterial BackdropMaterial { get; internal set; }
		public SystemBackdropTheme Theme { get; internal set; }

		public bool ShouldOverride { get; internal set; }
		public Color FallbackColor { get; internal set; }
		public Color TintColor { get; internal set; }
		public float TintOpacity { get; internal set; }
		public float LuminosityOpacity { get; internal set; }
	}


	[SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "Consumers intentionally use the cohesive EasyWindows.WindowOptions API.")]
	public sealed class WindowOptions {
		/// <summary>Creates fresh content on opening and every reload. Disposable content is disposed on replacement or close.</summary>
		public required Func<UIElement> ContentFactory { get; init; }
		public Action? Closed { get; init; }
		public required string Title { get; init; }
		public Func<string>? TitleProvider { get; init; }
		public required SizeInt32 DefaultSize { get; init; }
		public SizeInt32 MinimumSize { get; init; }
		public SizeInt32 MaximumSize { get; init; }
		public AppWindowPresenterKind PresenterKind { get; init; } = AppWindowPresenterKind.Default;
		public ButtonOptions? Button { get; init; }
		public bool IsResizable { get; init; } = true;
		public bool IsMinimizable { get; init; } = true;
		public bool IsMaximizable { get; init; } = true;
		public PointInt32? DefaultPosition { get; init; }
	}

}
