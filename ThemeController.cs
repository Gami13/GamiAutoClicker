using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using Windows.UI;
using WinRT;

namespace GamiAutoClicker;

public class ThemeController {
	WindowsSystemDispatcherQueueHelper? wsdqHelper; // See the helper class sample for the implementation
	MicaController? micaController;
	DesktopAcrylicController? acrylicController;
	SystemBackdropConfiguration? configurationSource;
	Window? window;

	public bool TrySetTheme(Window newWindow) {
		window = newWindow;
		//TODO: Dont block if no mica
		if (!MicaController.IsSupported()) return false;

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


		return true;



	}
	public void SetOverrides() {
		createController();
	}
	public void SetType() {
		createController();
	}
	public void SetTheme() {
		if (configurationSource != null) {

			if (window?.Content is FrameworkElement root) {
				var desired = MainWindow.themeSettings.theme switch {
					SystemBackdropTheme.Light => ElementTheme.Light,
					SystemBackdropTheme.Dark => ElementTheme.Dark,
					_ => ElementTheme.Default
				};
				root.RequestedTheme = desired;


			}
		}
	}
	public void SetMicaKind() {
		if (micaController != null) {
			micaController.Kind = MainWindow.themeSettings.micaKind;
		}
	}
	public void SetAcrylicKind() {
		if (acrylicController != null) {
			acrylicController.Kind = MainWindow.themeSettings.acrylicKind;
		}
	}

	public void SetFallbackColor() {
		if (!MainWindow.themeSettings.shouldOverride) return;
		if (micaController != null) {
			micaController.FallbackColor = MainWindow.themeSettings.fallbackColor;
		}
		if (acrylicController != null) {
			acrylicController.FallbackColor = MainWindow.themeSettings.fallbackColor;
		}
	}
	public void SetTintColor() {
		if (!MainWindow.themeSettings.shouldOverride) return;

		if (micaController != null) {
			micaController.TintColor = MainWindow.themeSettings.tintColor;
		}
		if (acrylicController != null) {
			acrylicController.TintColor = MainWindow.themeSettings.tintColor;
		}
		Debug.WriteLine("Updating tint color to: " + MainWindow.themeSettings.tintColor);

	}
	public void SetTintOpacity() {
		if (!MainWindow.themeSettings.shouldOverride) return;

		if (micaController != null) {
			micaController.TintOpacity = MainWindow.themeSettings.tintOpacity;
		}
		if (acrylicController != null) {
			acrylicController.TintOpacity = MainWindow.themeSettings.tintOpacity;
		}
		SetTintColor();
		Debug.WriteLine("Updating tint opacity to: " + MainWindow.themeSettings.tintOpacity);
	}
	public void SetLuminosityOpacity() {
		if (!MainWindow.themeSettings.shouldOverride) return;

		if (micaController != null) {
			micaController.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
		}
		if (acrylicController != null) {
			acrylicController.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
		}
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
		if (micaController != null) {
			micaController.Dispose();
			micaController = null;
		}
		if (acrylicController != null) {
			acrylicController.Dispose();
			acrylicController = null;
		}
	}

	private void createMicaController() {
		destroyController();
		micaController = new MicaController();
		micaController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		micaController.SetSystemBackdropConfiguration(configurationSource);
		micaController.Kind = MainWindow.themeSettings.micaKind;
		if (MainWindow.themeSettings.shouldOverride) {
			micaController.FallbackColor = MainWindow.themeSettings.fallbackColor;
			micaController.TintColor = MainWindow.themeSettings.tintColor;
			micaController.TintOpacity = MainWindow.themeSettings.tintOpacity;
			micaController.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
		}
	}

	private void createAcrylicController() {
		destroyController();
		acrylicController = new DesktopAcrylicController();
		acrylicController.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		acrylicController.SetSystemBackdropConfiguration(configurationSource);
		acrylicController.Kind = MainWindow.themeSettings.acrylicKind;
		if (MainWindow.themeSettings.shouldOverride) {
			acrylicController.FallbackColor = MainWindow.themeSettings.fallbackColor;
			acrylicController.TintColor = MainWindow.themeSettings.tintColor;
			acrylicController.TintOpacity = MainWindow.themeSettings.tintOpacity;
			acrylicController.LuminosityOpacity = MainWindow.themeSettings.luminosityOpacity;
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
		configurationSource.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
	}

	private void Window_Closed(object sender, WindowEventArgs args) {
		// Make sure any Mica/Acrylic controller is disposed
		if (micaController != null) {
			micaController.Dispose();
			micaController = null;
		}
		if (acrylicController != null) {
			acrylicController.Dispose();
			acrylicController = null;
		}
		window.Activated -= Window_Activated;
		configurationSource = null;
	}

	private void Window_ThemeChanged(FrameworkElement sender, object args) {
		if (configurationSource != null) {
			SetConfigurationSourceTheme();
		}
	}

	private void SetConfigurationSourceTheme() {
		switch (((FrameworkElement)window.Content).ActualTheme) {
			case ElementTheme.Dark: configurationSource.Theme = SystemBackdropTheme.Dark; break;
			case ElementTheme.Light: configurationSource.Theme = SystemBackdropTheme.Light; break;
			case ElementTheme.Default: configurationSource.Theme = SystemBackdropTheme.Default; break;
		}
	}
}
