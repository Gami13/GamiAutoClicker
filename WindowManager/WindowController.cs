using GamiAutoClicker.Components;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;

using WinRT.Interop;

namespace GamiAutoClicker.WindowManager;

public class WindowController : IDisposable {
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
	public static Dictionary<WindowKey, WindowController> windows = new();



	private readonly WindowKey _windowKey;
	private readonly WindowsSystemDispatcherQueueHelper _dispatcherHelper;
	private readonly SystemBackdropConfiguration backdropConfig;
	private readonly TopWindowBar _topWindowBar;
	public BackdropController? Backdrop { get; private set; }
	private bool _disposed;

	public Window? Window { get; private set; }

	public WindowController(Window window, WindowKey windowKey) {
		_windowKey = windowKey;
		Window = window;

		_dispatcherHelper = new WindowsSystemDispatcherQueueHelper();
		_dispatcherHelper.EnsureWindowsSystemDispatcherQueueController();

		backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
		UpdateConfigTheme();

		Backdrop = new BackdropController(window, backdropConfig);

		_topWindowBar = new TopWindowBar(windowKey);
		if (window.Content is Grid grid) {
			grid.Children.Insert(0, _topWindowBar);
			window.SetTitleBar(_topWindowBar);
		}

		var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
		var config = Constants.WindowConfigs[windowKey];
		appWindow.SetPresenter(config.presenterKind);
		appWindow.Resize(config.defaultSize);
		appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
		appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;

		if (appWindow.Presenter is OverlappedPresenter presenter) {
			presenter.IsMaximizable = config.isMaximizable;
			presenter.IsMinimizable = config.isMinimizable;
			presenter.IsResizable = config.isResizable;
		}

		window.Activated += OnActivated;
		window.Closed += OnClosed;
		if (window.Content is FrameworkElement root) root.ActualThemeChanged += OnThemeChanged;

		windows[windowKey] = this;
	}

	public void SetOverrides() {
		if (ThemeSettings.isFirstTimeOverriding) {
			ThemeSettings.isFirstTimeOverriding = false;

			ThemeSettings.fallbackColor = Backdrop?.GetFallbackColor() ?? Colors.Red;
			ThemeSettings.tintColor = Backdrop?.GetTintColor() ?? Colors.Red;
			ThemeSettings.tintOpacity = Backdrop?.GetTintOpacity() ?? 0f;
			ThemeSettings.luminosityOpacity = Backdrop?.GetLuminosityOpacity() ?? 0f;
		}
		Backdrop?.CreateController();
	}

	public void SetTheme() {
		if (Window?.Content is FrameworkElement root)
			root.RequestedTheme = ThemeSettings.theme switch {
				SystemBackdropTheme.Light => ElementTheme.Light,
				SystemBackdropTheme.Dark => ElementTheme.Dark,
				_ => ElementTheme.Default
			};
	}

	private void UpdateConfigTheme() {
		if (Window?.Content is FrameworkElement root)
			backdropConfig.Theme = root.ActualTheme switch {
				ElementTheme.Dark => SystemBackdropTheme.Dark,
				ElementTheme.Light => SystemBackdropTheme.Light,
				_ => SystemBackdropTheme.Default
			};
	}

	private void OnActivated(object sender, WindowActivatedEventArgs args) {
		backdropConfig.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
		_topWindowBar.Foreground = (SolidColorBrush)App.Current.Resources[
			args.WindowActivationState == WindowActivationState.Deactivated
				? "WindowCaptionForegroundDisabled" : "WindowCaptionForeground"];
	}

	private void OnThemeChanged(FrameworkElement sender, object args) => UpdateConfigTheme();
	private void OnClosed(object sender, WindowEventArgs args) => Dispose();

	public void Dispose() {
		if (_disposed) return;
		_disposed = true;

		Backdrop?.Dispose();
		Backdrop = null;
		windows.Remove(_windowKey);
		(_dispatcherHelper as IDisposable)?.Dispose();

		if (Window != null) {
			Window.Activated -= OnActivated;
			Window.Closed -= OnClosed;
			if (Window.Content is FrameworkElement root) root.ActualThemeChanged -= OnThemeChanged;
			Window.Close();
		}
		Window = null;
		GC.SuppressFinalize(this);
	}
}
