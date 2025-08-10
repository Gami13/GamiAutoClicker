using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using WinRT.Interop;

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
	public Color fallbackColor;
	public Color tintColor;
	public float tintOpacity;
	public float luminosityOpacity;
}

public sealed partial class MainWindow : Window {
	public static ThemeController mainWindowTheme = new();
	public static ThemeController settingsWindowTheme = new();
	public static ThemeSettings themeSettings = new() {
		type = ThemeType.Acrylic,
		micaKind = MicaKind.Base,
		acrylicKind = DesktopAcrylicKind.Default,
		theme = SystemBackdropTheme.Default,
		shouldOverride = false,
		fallbackColor = Colors.Transparent,
		tintColor = Colors.Transparent,
		tintOpacity = 0.0f,
		luminosityOpacity = 0.0f
	};


	private async void OpenSettingsWindow(object sender, RoutedEventArgs e) {

		var win = new Window();
		var frame = new Frame();
		frame.Navigate(typeof(SettingsPage));
		win.Content = frame;


		settingsWindowTheme.TrySetTheme(win);


		var hwnd = WindowNative.GetWindowHandle(win);
		var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
		var appWindow = AppWindow.GetFromWindowId(windowId);


		appWindow.Resize(new Windows.Graphics.SizeInt32(400, 300));
		appWindow.Move(new Windows.Graphics.PointInt32(200, 200));


		appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
		//applyWindowStyle(win);
		win.Activate();

	}


	public MainWindow() {
		InitializeComponent();



		mainWindowTheme.TrySetTheme(this);
		OpenSettingsWindow(null, null);
	}
}

