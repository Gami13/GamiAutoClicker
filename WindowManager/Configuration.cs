using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.Graphics;

namespace GamiAutoClicker.WindowManager {
	public class Configuration {
		public static ThemeSettings ThemeSettings = new() {
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
		public static Dictionary<object, WindowController> Windows = new();
		public static Dictionary<object, WindowManager.WindowConfig> WindowConfigs = new();
	}
}
