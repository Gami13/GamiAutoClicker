using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Gami;

public static partial class EasyWindows {
	internal sealed class WindowController : IDisposable {



		private readonly object _windowKey;
		private readonly SystemBackdropConfiguration _backdropConfig;
		private readonly TitleBar _topWindowBar;
		private readonly ContentControl _contentHost;
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
			if (!WindowConfigs.TryGetValue(windowKey, out WindowOptions? options)) {
				throw new ArgumentException($"WindowOptions for {windowKey} not found.");
			}
			_windowKey = windowKey;
			Window = window;

			WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();

			_topWindowBar = new TitleBar(windowKey);
			RefreshText();
			_contentHost = new ContentControl {
				HorizontalContentAlignment = HorizontalAlignment.Stretch,
				VerticalContentAlignment = VerticalAlignment.Stretch
			};
			Grid root = CreateRoot(window, _topWindowBar, _contentHost);

			_backdropConfig = new SystemBackdropConfiguration { IsInputActive = true };
			SetTheme();

			CreateAdapter();
			ReloadContent();

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

		internal void RefreshText() {
			WindowOptions options = GetWindowOptions(_windowKey);
			Window.Title = options.TitleProvider?.Invoke() ?? options.Title;
			_topWindowBar.RefreshText();
		}

		internal void ValidateReload() {
			if (!Window.DispatcherQueue.HasThreadAccess)
				throw new InvalidOperationException("ReloadAllWindows must run on the windows' UI thread.");
		}

		internal void ReloadContent() {
			ValidateReload();
			if (_disposed) return;
			UIElement replacement = GetWindowOptions(_windowKey).ContentFactory()
				?? throw new InvalidOperationException("ContentFactory must return fresh content.");
			object previous = _contentHost.Content;
			if (ReferenceEquals(previous, replacement))
				throw new InvalidOperationException("ContentFactory must return a new content instance on every call.");
			_contentHost.Content = replacement;
			(previous as IDisposable)?.Dispose();
			RefreshText();
		}

		private static Grid CreateRoot(Window window, TitleBar titleBar, ContentControl contentHost) {
			var root = new Grid();
			root.RowDefinitions.Add(new RowDefinition {
				Height = GridLength.Auto
			});
			root.RowDefinitions.Add(new RowDefinition {
				Height = new GridLength(1, GridUnitType.Star)
			});

			root.Children.Add(titleBar);
			Grid.SetRow(contentHost, 1);
			root.Children.Add(contentHost);

			window.Content = root;
			window.SetTitleBar(titleBar);

			return root;
		}

		public void CreateAdapter() {
			IBackdropAdapter? replacement = Theme.BackdropMaterial switch {
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

		internal bool TryCaptureBackdropDefaults() {
			if (Adapter is not { } adapter) return false;

			Theme.FallbackColor = adapter.FallbackColor;
			Theme.TintColor = adapter.TintColor;
			Theme.TintOpacity = adapter.TintOpacity;
			Theme.LuminosityOpacity = adapter.LuminosityOpacity;
			return true;
		}

		public void SetTheme() {
			if (Window.Content is FrameworkElement root) {
				ElementTheme requestedTheme = Theme.Theme switch {
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
				Window.AppWindow.TitleBar.PreferredTheme = root.ActualTheme == ElementTheme.Dark
					? TitleBarTheme.Dark
					: TitleBarTheme.Light;
			}
		}

		private void OnActivated(object sender, WindowActivatedEventArgs args) {
			bool isActive = args.WindowActivationState != WindowActivationState.Deactivated;
			_backdropConfig.IsInputActive = isActive;
			_topWindowBar.SetActive(isActive);
		}

		private void OnThemeChanged(FrameworkElement sender, object args) {
			SetTheme();
		}

		private void OnClosed(object sender, WindowEventArgs args) {
			_windowClosed = true;
			DisposeCore();
			GetWindowOptions(_windowKey).Closed?.Invoke();
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
			(_contentHost.Content as IDisposable)?.Dispose();
			_contentHost.Content = null;

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
