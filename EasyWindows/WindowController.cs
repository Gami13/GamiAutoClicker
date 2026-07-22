using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

using WinRT.Interop;

namespace Gami;

public static partial class EasyWindows {
	public class WindowController : IDisposable {



		private readonly object _windowKey;
		private readonly WindowsSystemDispatcherQueueHelper _dispatcherHelper;
		private readonly SystemBackdropConfiguration _backdropConfig;
		private readonly TitleBar _topWindowBar;
		public BackdropController? Backdrop { get; private set; }
		private bool _disposed;
		private bool _closing;

		public Window? Window { get; private set; }

		// Register creates a WindowController and stores it in Manager.Windows[key].
		// The constructor itself is the factory — no caller holds the reference;
		// the dictionary entry is the sole owner of the object's lifetime.
		public static void Register(Window window, object windowKey) =>
			new WindowController(window, windowKey);

		private WindowController(Window window, object windowKey) {
			if (!Manager.WindowConfigs.ContainsKey(windowKey)) {
				throw new ArgumentException($"WindowConfig for {windowKey} not found.");
			}
			_windowKey = windowKey;
			Window = window;

			_dispatcherHelper = new WindowsSystemDispatcherQueueHelper();
			_dispatcherHelper.EnsureWindowsSystemDispatcherQueueController();

			_backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
			UpdateConfigTheme();

			Backdrop = new BackdropController(window, _backdropConfig);

			_topWindowBar = new TitleBar(windowKey);
			if (window != null && window.Content is Grid grid) {
				grid.Children.Insert(0, _topWindowBar);
				window.SetTitleBar(_topWindowBar);
			}

			var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
			var config = EasyWindows.Manager.WindowConfigs[windowKey];
			appWindow.SetPresenter(config.presenterKind);
			appWindow.Resize(config.defaultSize);
			if ( config.defaultPosition.Width >= 0
				&& config.defaultPosition.Height >= 0) {

				appWindow.Move(new Windows.Graphics.PointInt32(config.defaultPosition.Width, config.defaultPosition.Height));
			}
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

			Manager.Windows[windowKey] = this;
		}

		public void SetOverrides() {
			if (Manager.ThemeSettings.isFirstTimeOverriding) {
				Manager.ThemeSettings.isFirstTimeOverriding = false;

				Manager.ThemeSettings.fallbackColor = Backdrop?.GetFallbackColor() ?? Colors.Red;
				Manager.ThemeSettings.tintColor = Backdrop?.GetTintColor() ?? Colors.Red;
				Manager.ThemeSettings.tintOpacity = Backdrop?.GetTintOpacity() ?? 0f;
				Manager.ThemeSettings.luminosityOpacity = Backdrop?.GetLuminosityOpacity() ?? 0f;
			}
			Backdrop?.CreateController();
		}

		public void SetTheme() {
			if (Window?.Content is FrameworkElement root)
				root.RequestedTheme = Manager.ThemeSettings.theme switch {
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

			string resourceKey = args.WindowActivationState == WindowActivationState.Deactivated
		 ? "WindowCaptionForegroundDisabled"
		 : "WindowCaptionForeground";

			if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush) {
				_topWindowBar.Foreground = brush;
			}
		}

		private void OnThemeChanged(FrameworkElement sender, object args) => UpdateConfigTheme();
		private void OnClosed(object sender, WindowEventArgs args) {
			_closing = true;
			((IDisposable)this).Dispose();
		}

		public void Dispose() {
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing) {
			if (_disposed) return;

			if (disposing) {
				Backdrop?.Dispose();
				Backdrop = null;
				Manager.Windows.Remove(_windowKey);
				(_dispatcherHelper as IDisposable)?.Dispose();

				if (Window != null) {
					Window.Activated -= OnActivated;
					Window.Closed -= OnClosed;
					if (Window.Content is FrameworkElement root) root.ActualThemeChanged -= OnThemeChanged;
					if (!_closing) Window.Close();
				}
				Window = null;
			}

			_disposed = true;
		}
	}
}