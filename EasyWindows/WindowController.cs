using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Windows.Graphics;

namespace Gami;

public static partial class EasyWindows {
	internal sealed class WindowController : IDisposable {



		private readonly object _windowKey;
		private readonly SystemBackdropConfiguration _backdropConfig;
		private readonly TitleBar _topWindowBar;
		public IBackdropAdapter? Adapter { get; private set; }
		private bool _disposed;
		private bool _windowClosed;

		public Microsoft.UI.Xaml.Window Window { get; }


		internal static WindowController Register(Microsoft.UI.Xaml.Window window, object windowKey) {
			var controller = new WindowController(window, windowKey);
			WindowControllers[windowKey] = controller;
			return controller;
		}

		private WindowController(Window window, object windowKey) {
			if (!WindowConfigs.TryGetValue(windowKey, out var options)) {
				throw new ArgumentException($"WindowOptions for {windowKey} not found.");
			}
			_windowKey = windowKey;
			Window = window;

			WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

			_topWindowBar = new TitleBar(windowKey);
			var root = CreateRoot(window, _topWindowBar);

			_backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
			SetTheme();

			CreateAdapter();

			Window.AppWindow.SetPresenter(options.PresenterKind);
			Window.AppWindow.Resize(options.DefaultSize);
			if (options.DefaultPosition is { } position) {
				Window.AppWindow.Move(position);
			}
			Window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
			Window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;

			if (Window.AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.IsMaximizable = options.IsMaximizable;
				presenter.IsMinimizable = options.IsMinimizable;
				presenter.IsResizable = options.IsResizable;

				presenter.PreferredMinimumHeight = options.MinimumSize.Height;
				presenter.PreferredMinimumWidth = options.MinimumSize.Width;
				if (options.MaximumSize.Width > 0 && options.MaximumSize.Height > 0) {

					presenter.PreferredMaximumHeight = options.MaximumSize.Height;
					presenter.PreferredMaximumWidth = options.MaximumSize.Width;
				}
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
				if (content is FrameworkElement frameworkElement) {
					Grid.SetRow(frameworkElement, 1);
				} else {
					content.SetValue(Grid.RowProperty, 1);
				}
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
			CreateAdapter();
		}

		internal bool TryCaptureBackdropDefaults() {
			if (Adapter is not { } adapter) return false;

			Theme.fallbackColor = adapter.FallbackColor;
			Theme.tintColor = adapter.TintColor;
			Theme.tintOpacity = adapter.TintOpacity;
			Theme.luminosityOpacity = adapter.LuminosityOpacity;
			return true;
		}

		public void SetTheme() {
			if (Window.Content is FrameworkElement root) {
				var requestedTheme = Theme.theme switch {
					SystemBackdropTheme.Light => ElementTheme.Light,
					SystemBackdropTheme.Dark => ElementTheme.Dark,
					_ => ElementTheme.Default
				};
				if (root.RequestedTheme != requestedTheme) {
					root.RequestedTheme = requestedTheme;
				}

				_backdropConfig.Theme = root.ActualTheme switch {
					ElementTheme.Dark => SystemBackdropTheme.Dark,
					ElementTheme.Light => SystemBackdropTheme.Light,
					_ => SystemBackdropTheme.Default
				};
			}
		}

		private void OnActivated(object sender, WindowActivatedEventArgs args) {
			bool isActive = args.WindowActivationState != WindowActivationState.Deactivated;
			_backdropConfig.IsInputActive = isActive;
			_topWindowBar.RefreshForeground(isActive);
		}

		private void OnThemeChanged(FrameworkElement sender, object args) {
			SetTheme();
			_topWindowBar.RefreshForeground();
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

			WindowControllers.Remove(_windowKey);
		}
	}
}
