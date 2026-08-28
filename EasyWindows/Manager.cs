using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gami;

public static partial class EasyWindows {
	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static ThemeSettings Theme { get; } = new() {
		backdropMaterial = BackdropMaterial.Acrylic,
		theme = SystemBackdropTheme.Default,
		shouldOverride = false,
		isFirstTimeOverriding = false,
		fallbackColor = Colors.White,
		tintColor = Colors.White,
		tintOpacity = 0.0f,
		luminosityOpacity = 0.0f
	};

	private static Dictionary<object, WindowController> Windows { get; } = new();
	private static Dictionary<object, WindowOptions> WindowConfigs { get; } = new();

	public static void RegisterWindow(object key, WindowOptions options) {
		ArgumentNullException.ThrowIfNull(options);

		if (!WindowConfigs.TryAdd(key, options)) {
			throw new ArgumentException($"Window {key} is already registered.", nameof(key));
		}
	}
}
