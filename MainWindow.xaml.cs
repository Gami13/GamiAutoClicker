using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Graphics;
using Windows.UI;
using Windows.UI.ApplicationSettings;
using Windows.UI.WindowManagement;
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
	private Microsoft.UI.Windowing.AppWindow appWindow;
	public static ThemeController? mainWindowTheme;
	public static ThemeController? settingsWindowTheme;
	public static SettingsWindow? settingsWindowInstance;
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


	private void OpenSettingsWindow(object? sender, RoutedEventArgs? e) {
		if (settingsWindowInstance != null) {
			return;
		}

		var settingsWindow = new SettingsWindow();
		settingsWindowTheme = new ThemeController(settingsWindow);

		var hwnd = WindowNative.GetWindowHandle(settingsWindow);
		var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
		var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

		appWindow.Resize(new SizeInt32(400, 300));
		appWindow.Move(new PointInt32(200, 200));

		appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
		settingsWindow.Activate();
		settingsWindowInstance = settingsWindow;
	}


	public MainWindow() {
		InitializeComponent();



		mainWindowTheme = new ThemeController(this);
		//OpenSettingsWindow(null, null);

		appWindow = this.AppWindow;
		AppTitleBar.Loaded += AppTitleBar_Loaded;
		AppTitleBar.SizeChanged += AppTitleBar_SizeChanged;
		Activated += MainWindow_Activated;
		Closed += MainWindow_Closed;
		ExtendsContentIntoTitleBar = true;
		TitleBarTextBlock.Text = AppInfo.Current.DisplayInfo.DisplayName;
		AppTitleBar.Height = 32;
		SetTitleBar(AppTitleBar);
	}

	private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
		if (args.WindowActivationState == WindowActivationState.Deactivated) {
			TitleBarSettingsButton.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
			//TitleBarIcon.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
			TitleBarTextBlock.Foreground =
				(SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
		}
		else {
			TitleBarSettingsButton.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
			//TitleBarIcon.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];
			TitleBarTextBlock.Foreground =
				(SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];

		}
	}

	private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) {
		if (ExtendsContentIntoTitleBar == true) {
			SetRegionsForCustomTitleBar();
		}
	}

	private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) {
		if (ExtendsContentIntoTitleBar == true) {
			SetRegionsForCustomTitleBar();
		}
	}

	private void SetRegionsForCustomTitleBar() {
		// Specify the interactive regions of the title bar.
		double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

		RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
		LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);


		GeneralTransform transform = TitleBarSettingsButton.TransformToVisual(null);
		Rect bounds = transform.TransformBounds(new Rect(0, 0, TitleBarSettingsButton.ActualWidth, TitleBarSettingsButton.ActualHeight));
		RectInt32 SettingsButtonRect = GetRect(bounds, scaleAdjustment);



		var rectArray = new RectInt32[] { SettingsButtonRect };

		InputNonClientPointerSource nonClientInputSrc =
			InputNonClientPointerSource.GetForWindowId(this.AppWindow.Id);
		nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);

	}


	private RectInt32 GetRect(Rect bounds, double scale) {
		return new Windows.Graphics.RectInt32(
			_X: (int)Math.Round(bounds.X * scale),
			_Y: (int)Math.Round(bounds.Y * scale),
			_Width: (int)Math.Round(bounds.Width * scale),
			_Height: (int)Math.Round(bounds.Height * scale)
		);
	}

	private void MainWindow_Closed(object? sender, WindowEventArgs args) {
		mainWindowTheme?.Dispose();
		settingsWindowTheme?.Dispose();
		settingsWindowInstance?.Close();
		mainWindowTheme = null;
		settingsWindowTheme = null;
		settingsWindowInstance = null;
		Application.Current.Exit();


	}
}

