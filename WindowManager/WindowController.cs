using GamiAutoClicker.Components;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

using WinRT.Interop;

namespace GamiAutoClicker.WindowManager;

public class WindowController : IDisposable {



	private readonly WindowKey _windowKey;
	private readonly WindowsSystemDispatcherQueueHelper _dispatcherHelper;
	private readonly SystemBackdropConfiguration _backdropConfig;
	private readonly TopWindowBar _topWindowBar;
	public BackdropController? Backdrop { get; private set; }
	private bool _disposed;

	public Window? Window { get; private set; }

	public WindowController(Window window, WindowKey windowKey) {
		if(!Configuration.WindowConfigs.ContainsKey(windowKey)) {
			throw new ArgumentException($"WindowConfig for {windowKey} not found.");
		}
		_windowKey = windowKey;
		Window = window;

		_dispatcherHelper = new WindowsSystemDispatcherQueueHelper();
		_dispatcherHelper.EnsureWindowsSystemDispatcherQueueController();

		_backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
		UpdateConfigTheme();

		Backdrop = new BackdropController(window, _backdropConfig);

		_topWindowBar = new TopWindowBar(windowKey);
		if (window.Content is Grid grid) {
			grid.Children.Insert(0, _topWindowBar);
			window.SetTitleBar(_topWindowBar);
		}

		var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
		var config = WindowManager.Configuration.WindowConfigs[windowKey];
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

		Configuration.Windows[windowKey] = this;
	}

	public void SetOverrides() {
		if (Configuration.ThemeSettings.isFirstTimeOverriding) {
			Configuration.ThemeSettings.isFirstTimeOverriding = false;

			Configuration.ThemeSettings.fallbackColor = Backdrop?.GetFallbackColor() ?? Colors.Red;
			Configuration.ThemeSettings.tintColor = Backdrop?.GetTintColor() ?? Colors.Red;
			Configuration.ThemeSettings.tintOpacity = Backdrop?.GetTintOpacity() ?? 0f;
			Configuration.ThemeSettings.luminosityOpacity = Backdrop?.GetLuminosityOpacity() ?? 0f;
		}
		Backdrop?.CreateController();
	}

	public void SetTheme() {
		if (Window?.Content is FrameworkElement root)
			root.RequestedTheme = Configuration.ThemeSettings.theme switch {
				SystemBackdropTheme.Light => ElementTheme.Light,
				SystemBackdropTheme.Dark => ElementTheme.Dark,
				_ => ElementTheme.Default
			};
	}

	private void UpdateConfigTheme() {
		if (Window?.Content is FrameworkElement root)
			_backdropConfig.Theme = root.ActualTheme switch {
				ElementTheme.Dark => SystemBackdropTheme.Dark,
				ElementTheme.Light => SystemBackdropTheme.Light,
				_ => SystemBackdropTheme.Default
			};
	}

	private void OnActivated(object sender, WindowActivatedEventArgs args) {
		_backdropConfig.IsInputActive = args.WindowActivationState != WindowActivationState.Deactivated;
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
		Configuration.Windows.Remove(_windowKey);
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
