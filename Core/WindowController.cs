using GamiAutoClicker.Components;
using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Windows.Graphics;
using Windows.UI;
using WinRT;

namespace GamiAutoClicker;

public class WindowController {
	public Window? window;
	WindowsSystemDispatcherQueueHelper? wsdqHelper;
	SystemBackdropConfiguration? configurationSource;
	WindowKey windowKey;
	ISystemBackdropController? controller;
	TopWindowBar topWindowBar;

	private bool _disposed = false;

	private Func<Color>? _getFallbackColor;
	private Action<Color>? _setFallbackColor;

	private Func<Color>? _getTintColor;
	private Action<Color>? _setTintColor;

	private Func<float>? _getTintOpacity;
	private Action<float>? _setTintOpacity;

	private Func<float>? _getLuminosityOpacity;
	private Action<float>? _setLuminosityOpacity;

	public WindowController(Window newWindow, WindowKey windowKey) {
		this.windowKey = windowKey;
		window = newWindow;
		//TODO: Dont block if no mica
		//if (!MicaController.IsSupported());

		wsdqHelper = new WindowsSystemDispatcherQueueHelper();
		wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

		configurationSource = new SystemBackdropConfiguration();
		configurationSource.IsInputActive = true;

		SetConfigurationSourceTheme();
		createController();
		var config = Constants.WindowConfigs[windowKey];

		topWindowBar = new TopWindowBar(windowKey);
		if (window.Content is Grid rootGrid) {
			rootGrid.Children.Insert(0, topWindowBar);
			window.SetTitleBar(topWindowBar);
		}


		IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
		AppWindow appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd));
		appWindow.SetPresenter(config.presenterKind);
		applyWindowStyle(appWindow);

		window.Activated += Window_Activated;
		window.Closed += Window_Closed;
		((FrameworkElement)window.Content).ActualThemeChanged += Window_ThemeChanged;

	}

	public void SetOverrides() {
		if (MainWindow.themeSettings.isFirstTimeOverriding) {
			MainWindow.themeSettings.isFirstTimeOverriding = false;
			MainWindow.themeSettings.fallbackColor = _getFallbackColor?.Invoke() ?? Colors.Red;
			MainWindow.themeSettings.tintColor = _getTintColor?.Invoke() ?? Colors.Red;
			MainWindow.themeSettings.tintOpacity = _getTintOpacity?.Invoke() ?? 0.0f;
			MainWindow.themeSettings.luminosityOpacity = _getLuminosityOpacity?.Invoke() ?? 0.0f;

		}

		if (MainWindow.windowThemes[WindowKey.Settings].window is SettingsWindow settingsWindow) {
			settingsWindow.UpdateSwitches();
		}

		createController();
	}
	public void SetType() {
		createController();
	}

	public void SetTheme() {
		if (configurationSource != null && window?.Content is FrameworkElement root) {
			root.RequestedTheme = MainWindow.themeSettings.theme switch {
				SystemBackdropTheme.Light => ElementTheme.Light,
				SystemBackdropTheme.Dark => ElementTheme.Dark,
				_ => ElementTheme.Default
			};
		}
	}

	public void SetMicaKind() {
		if (controller is MicaController mc) {
			mc.Kind = MainWindow.themeSettings.micaKind;
		}
	}

	public void SetAcrylicKind() {
		if (controller is DesktopAcrylicController ac) {
			ac.Kind = MainWindow.themeSettings.acrylicKind;
		}
	}

	public void SetFallbackColor() {
		if (!MainWindow.themeSettings.shouldOverride) return;
		_setFallbackColor?.Invoke(MainWindow.themeSettings.fallbackColor);
	}

	public void SetTintColor() {
		if (!MainWindow.themeSettings.shouldOverride) return;
		_setTintColor?.Invoke(MainWindow.themeSettings.tintColor);
	}

	public void SetTintOpacity() {
		if (!MainWindow.themeSettings.shouldOverride) return;
		_setTintOpacity?.Invoke(MainWindow.themeSettings.tintOpacity);

		//! Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
		if (_getTintColor == null || _setTintColor == null) return;

		var currentColor = _getTintColor();
		var tempColor = currentColor;
		if (tempColor.A < 255) tempColor.A = (byte)(tempColor.A + 1);
		else if (tempColor.A > 0) tempColor.A = (byte)(tempColor.A - 1);

		_setTintColor(tempColor);
		_setTintColor(currentColor);
	}

	public void SetLuminosityOpacity() {
		if (!MainWindow.themeSettings.shouldOverride) return;
		_setLuminosityOpacity?.Invoke(MainWindow.themeSettings.luminosityOpacity);
	}

	private void createController() {
		if (MainWindow.themeSettings.type == ThemeType.Mica) {
			createMicaController();
		}
		if (MainWindow.themeSettings.type == ThemeType.Acrylic) {
			createAcrylicController();
		}
	}

	private void destroyController() {
		if (controller is IDisposable disposable) {
			disposable.Dispose();
		}
		controller = null;
		_getFallbackColor = null;
		_setFallbackColor = null;
		_getTintColor = null;
		_setTintColor = null;
		_getTintOpacity = null;
		_setTintOpacity = null;
		_getLuminosityOpacity = null;
		_setLuminosityOpacity = null;
	}

	private void createMicaController() {
		destroyController();
		var mc = new MicaController();
		mc.Kind = MainWindow.themeSettings.micaKind;

		_getFallbackColor = () => mc.FallbackColor;
		_setFallbackColor = color => mc.FallbackColor = color;

		_getTintColor = () => mc.TintColor;
		_setTintColor = color => mc.TintColor = color;

		_getTintOpacity = () => mc.TintOpacity;
		_setTintOpacity = opacity => mc.TintOpacity = opacity;

		_getLuminosityOpacity = () => mc.LuminosityOpacity;
		_setLuminosityOpacity = opacity => mc.LuminosityOpacity = opacity;

		initializeController(mc);
		controller = mc;
	}

	private void createAcrylicController() {
		destroyController();
		var ac = new DesktopAcrylicController();
		ac.Kind = MainWindow.themeSettings.acrylicKind;


		_getFallbackColor = () => ac.FallbackColor;
		_setFallbackColor = color => ac.FallbackColor = color;

		_getTintColor = () => ac.TintColor;
		_setTintColor = color => ac.TintColor = color;

		_getTintOpacity = () => ac.TintOpacity;
		_setTintOpacity = opacity => ac.TintOpacity = opacity;

		_getLuminosityOpacity = () => ac.LuminosityOpacity;
		_setLuminosityOpacity = opacity => ac.LuminosityOpacity = opacity;



		initializeController(ac);
		controller = ac;
	}

	private void initializeController(MicaController controller) {
		controller.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		controller.SetSystemBackdropConfiguration(configurationSource);
		if (MainWindow.themeSettings.shouldOverride) {
			controller.FallbackColor = MainWindow.themeSettings.fallbackColor;
			controller.TintColor = MainWindow.themeSettings.tintColor;
			controller.TintOpacity = MainWindow.themeSettings.tintOpacity;
			controller.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
		}
	}
	private void initializeController(DesktopAcrylicController controller) {
		controller.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		controller.SetSystemBackdropConfiguration(configurationSource);
		if (MainWindow.themeSettings.shouldOverride) {
			controller.FallbackColor = MainWindow.themeSettings.fallbackColor;
			controller.TintColor = MainWindow.themeSettings.tintColor;
			controller.TintOpacity = MainWindow.themeSettings.tintOpacity;
			controller.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
		}
	}

	private void applyWindowStyle(AppWindow appWindow) {
		var config = Constants.WindowConfigs[windowKey];

		appWindow.Resize(config.defaultSize);


		if (appWindow.Presenter is OverlappedPresenter presenter) {
			presenter.IsMaximizable = config.isMaximizable;
			presenter.IsMinimizable = config.isMinimizable;
			presenter.IsResizable = config.isResizable;
		}

		appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
		appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
		appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs args) {
		if (configurationSource != null)
			configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;


		if (args.WindowActivationState == WindowActivationState.Deactivated) {
			topWindowBar.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForegroundDisabled"];
		}
		else {
			topWindowBar.Foreground = (SolidColorBrush)App.Current.Resources["WindowCaptionForeground"];

		}
	}



	private void Window_ThemeChanged(FrameworkElement sender, object args) {
		if (configurationSource != null) {
			SetConfigurationSourceTheme();
		}
	}

	private void SetConfigurationSourceTheme() {
		if (configurationSource != null && window?.Content is FrameworkElement root) {
			configurationSource.Theme = root.ActualTheme switch {
				ElementTheme.Dark => SystemBackdropTheme.Dark,
				ElementTheme.Light => SystemBackdropTheme.Light,
				_ => SystemBackdropTheme.Default
			};
		}
	}
	private void Window_Closed(object sender, WindowEventArgs args) {
		Dispose();
	}

	public void Dispose() {
		if (_disposed) return;

		destroyController();


		MainWindow.windowThemes.Remove(windowKey);

		configurationSource = null;

		if (wsdqHelper is IDisposable disposableHelper) {
			disposableHelper.Dispose();
		}
		wsdqHelper = null;

		// Mark as disposed and remove references
		if (window != null) {
			window.Activated -= Window_Activated;
			window.Closed -= Window_Closed;

			if (window.Content is FrameworkElement root) {
				root.ActualThemeChanged -= Window_ThemeChanged;
			}
			window.Close();

		}
		_disposed = true;
		window = null;
		GC.SuppressFinalize(this);
	}
}