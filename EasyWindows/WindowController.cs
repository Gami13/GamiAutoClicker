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
	internal sealed class WindowController : IDisposable {



		private readonly object _windowKey;
		private readonly WindowsSystemDispatcherQueueHelper _dispatcherHelper;
		private readonly SystemBackdropConfiguration _backdropConfig;
		private readonly TitleBar _topWindowBar;
		public IBackdropAdapter? Adapter { get; private set; }
		private bool _disposed;
		private bool _windowClosed;
		private bool _isWindowActive = true;

		public Window Window { get; }

		internal static WindowController Register(Window window, object windowKey) {
			var controller = new WindowController(window, windowKey);
			Windows[windowKey] = controller;
			return controller;
		}

		private WindowController(Window window, object windowKey) {
			if (!WindowConfigs.TryGetValue(windowKey, out var options)) {
				throw new ArgumentException($"WindowOptions for {windowKey} not found.");
			}
			_windowKey = windowKey;
			Window = window;

			_dispatcherHelper = new WindowsSystemDispatcherQueueHelper();
			_dispatcherHelper.EnsureWindowsSystemDispatcherQueueController();

			_topWindowBar = new TitleBar(windowKey);
			var root = CreateRoot(window, _topWindowBar);

			_backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
			UpdateConfigTheme();

			CreateAdapter();

			var appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(WindowNative.GetWindowHandle(window)));
			appWindow.SetPresenter(options.PresenterKind);
			appWindow.Resize(options.DefaultSize);
			if (options.DefaultPosition is { } position) {
				appWindow.Move(position);
			}
			appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
			appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;

			if (appWindow.Presenter is OverlappedPresenter presenter) {
				presenter.IsMaximizable = options.IsMaximizable;
				presenter.IsMinimizable = options.IsMinimizable;
				presenter.IsResizable = options.IsResizable;
			}

			window.Activated += OnActivated;
			window.Closed += OnClosed;
			root.ActualThemeChanged += OnThemeChanged;

		}

		private static Grid CreateRoot(Window window, TitleBar titleBar) {
			var content = window.Content as UIElement;
			var root = new Grid();
			root.RowDefinitions.Add(new RowDefinition {
				Height = GridLength.Auto
			});
			root.RowDefinitions.Add(new RowDefinition {
				Height = new GridLength(1, GridUnitType.Star)
			});

			window.Content = null;
			root.Children.Add(titleBar);

			if (content is not null) {
				content.SetValue(Grid.RowProperty, 1);
				root.Children.Add(content);
			}

			window.Content = root;
			window.SetTitleBar(titleBar);

			return root;
		}

		public void CreateAdapter() {
			IBackdropAdapter? replacement = Theme.backdropMaterial switch {
				BackdropMaterial.Mica when MicaController.IsSupported() => new MicaAdapter(Window, _backdropConfig, MicaKind.Base),
				BackdropMaterial.MicaAlt when MicaController.IsSupported() => new MicaAdapter(Window, _backdropConfig, MicaKind.BaseAlt),
				BackdropMaterial.Acrylic when DesktopAcrylicController.IsSupported() => new AcrylicAdapter(Window, _backdropConfig, DesktopAcrylicKind.Base),
				BackdropMaterial.AcrylicThin when DesktopAcrylicController.IsSupported() => new AcrylicAdapter(Window, _backdropConfig, DesktopAcrylicKind.Thin),
				_ => null
			};

			IBackdropAdapter? previous = Adapter;
			Adapter = replacement;
			previous?.Dispose();
		}

		public void SetOverrides() {
			if (Theme.isFirstTimeOverriding) {
				Theme.isFirstTimeOverriding = false;

				Theme.fallbackColor = Adapter?.FallbackColor ?? Colors.Red;
				Theme.tintColor = Adapter?.TintColor ?? Colors.Red;
				Theme.tintOpacity = Adapter?.TintOpacity ?? 0f;
				Theme.luminosityOpacity = Adapter?.LuminosityOpacity ?? 0f;
			}
			CreateAdapter();
		}

		public void SetTheme() {
			if (Window.Content is FrameworkElement root)
				root.RequestedTheme = Theme.theme switch {
					SystemBackdropTheme.Light => ElementTheme.Light,
					SystemBackdropTheme.Dark => ElementTheme.Dark,
					_ => ElementTheme.Default
				};
		}

		private void UpdateConfigTheme() {
			if (Window.Content is FrameworkElement root)
				_backdropConfig.Theme = root.ActualTheme switch {
					ElementTheme.Dark => SystemBackdropTheme.Dark,
					ElementTheme.Light => SystemBackdropTheme.Light,
					_ => SystemBackdropTheme.Default
				};
		}

		private void OnActivated(object sender, WindowActivatedEventArgs args) {
			_isWindowActive = args.WindowActivationState != WindowActivationState.Deactivated;
			_backdropConfig.IsInputActive = _isWindowActive;
			UpdateTitleBarForeground();
		}

		private void UpdateTitleBarForeground() {
			string resourceKey = _isWindowActive
				? "WindowCaptionForeground"
				: "WindowCaptionForegroundDisabled";

			if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush) {
				_topWindowBar.Foreground = brush;
			}
		}

		private void OnThemeChanged(FrameworkElement sender, object args) {
			UpdateConfigTheme();
			UpdateTitleBarForeground();
		}

		private void OnClosed(object sender, WindowEventArgs args) {
			_windowClosed = true;
			DisposeCore();
		}

		public void Dispose() {
			if (_disposed) {
				return;
			}

			DisposeCore();

			if (!_windowClosed) {
				Window.Close();
			}
		}

		private void DisposeCore() {
			if (_disposed) {
				return;
			}

			_disposed = true;

			Window.Activated -= OnActivated;
			Window.Closed -= OnClosed;
			if (Window.Content is FrameworkElement root)
				root.ActualThemeChanged -= OnThemeChanged;

			Adapter?.Dispose();
			Adapter = null;

			Windows.Remove(_windowKey);
		}
	}
}
