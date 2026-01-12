using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Windows.Graphics;

namespace GamiAutoClicker.WindowManager {
	[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
	public static class Configuration {
		public static ThemeSettings ThemeSettings { get; set; } = new() {
			type = ThemeType.Acrylic,
			micaKind = MicaKind.Base,
			acrylicKind = DesktopAcrylicKind.Default,
			theme = SystemBackdropTheme.Default,
			shouldOverride = false,
			isFirstTimeOverriding = true,
			fallbackColor = Colors.Transparent,
			tintColor = Colors.Transparent,
			tintOpacity = 0.0f,
			luminosityOpacity = 0.0f
		};
		public static Dictionary<object, WindowController> Windows { get; } = new();
		public static Dictionary<object, WindowManager.WindowConfig> WindowConfigs { get;  } = new();
	}
}
