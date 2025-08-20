using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics.CodeAnalysis;
using Windows.Graphics;
using Windows.UI;
using WinRT;

namespace GamiAutoClicker;

public class ThemeController {
	WindowsSystemDispatcherQueueHelper wsdqHelper;
	SystemBackdropConfiguration? configurationSource;
	Window window;

	ISystemBackdropController? controller;


	private Action<Color>? _setFallbackColor;
	private Action<Color>? _setTintColor;
	private Action<float>? _setTintOpacity;
	private Action<float>? _setLuminosityOpacity;
	private Func<Color>? _getTintColor; 

	public ThemeController(Window newWindow) {
		window = newWindow;
		//TODO: Dont block if no mica
		//if (!MicaController.IsSupported());

		wsdqHelper = new WindowsSystemDispatcherQueueHelper();
		wsdqHelper.EnsureWindowsSystemDispatcherQueueController();

		configurationSource = new SystemBackdropConfiguration();
		window.Activated += Window_Activated;
		window.Closed += Window_Closed;
		((FrameworkElement)window.Content).ActualThemeChanged += Window_ThemeChanged;

		configurationSource.IsInputActive = true;

		SetConfigurationSourceTheme();
		createController();

		IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
		AppWindow appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd));
		applyWindowStyle(appWindow);
	}

	public void SetOverrides() => createController();
	public void SetType() => createController();

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
		_setFallbackColor = null;
		_setTintColor = null;
		_setTintOpacity = null;
		_setLuminosityOpacity = null;
		_getTintColor = null;
	}

	private void createMicaController() {
		destroyController();
		var mc = new MicaController();
		mc.Kind = MainWindow.themeSettings.micaKind;

		// Assign delegates for MicaController
		_setFallbackColor = color => mc.FallbackColor = color;
		_setTintColor = color => mc.TintColor = color;
		_setTintOpacity = opacity => mc.TintOpacity = opacity;
		_setLuminosityOpacity = opacity => mc.LuminosityOpacity = opacity;
		_getTintColor = () => mc.TintColor;

		initializeController(mc);
		controller = mc;
	}

	private void createAcrylicController() {
		destroyController();
		var ac = new DesktopAcrylicController();
		ac.Kind = MainWindow.themeSettings.acrylicKind;

		// Assign delegates for DesktopAcrylicController
		_setFallbackColor = color => ac.FallbackColor = color;
		_setTintColor = color => ac.TintColor = color;
		_setTintOpacity = opacity => ac.TintOpacity = opacity;
		_setLuminosityOpacity = opacity => ac.LuminosityOpacity = opacity;
		_getTintColor = () => ac.TintColor;

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
		SizeInt32 windowSize = new SizeInt32 { Width = 370, Height = 290 };
		appWindow.Resize(windowSize);

		var presenter = appWindow.Presenter as OverlappedPresenter;
		if (presenter != null) {
			//presenter.IsResizable = false;
		}
		appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
		appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
		appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;
	}

	private void Window_Activated(object sender, WindowActivatedEventArgs args) {
		if (configurationSource != null)
			configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
	}

	private void Window_Closed(object sender, WindowEventArgs args) {
		destroyController();
		if (window != null) {
			window.Activated -= Window_Activated;
			window.Closed -= Window_Closed;
			if (window.Content is FrameworkElement root) {
				root.ActualThemeChanged -= Window_ThemeChanged;
			}
		}
		configurationSource = null;
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
}