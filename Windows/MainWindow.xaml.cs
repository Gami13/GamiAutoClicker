using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.WindowManagement;
using WinRT.Interop;

namespace GamiAutoClicker;




public sealed partial class MainWindow : Window {
	public static Dictionary<WindowKey,WindowController> windowThemes = new();
	public static ThemeSettings themeSettings = new() {
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


	public MainWindow() {
		InitializeComponent();
		windowThemes[WindowKey.Main] = new WindowController(this, WindowKey.Main);
		//windowThemes[WindowKey.Main].SetOverrides();
		Closed += MainWindow_Closed;
		ExtendsContentIntoTitleBar = true;


	}


	private void MainWindow_Closed(object? sender, WindowEventArgs args) {

		Application.Current.Exit();
		


	}
}

