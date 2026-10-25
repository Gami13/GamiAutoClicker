using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Reactor;
namespace Gami;

public static partial class EasyWindows {
	internal sealed class WindowController : IDisposable {
		private readonly object _windowKey;
		private readonly SystemBackdropConfiguration _backdropConfig = new() { IsInputActive = true };
		private bool _disposed, _windowClosed;
		public IBackdropAdapter? Adapter { get; private set; }
		public Window Window { get; }
		internal ReactorWindow ReactorWindow { get; }
		internal WindowController(ReactorWindow window, object key) {
			_windowKey = key;
			ReactorWindow = window;
			Window = window.NativeWindow;
			WindowsSystemDispatcherQueueHelper.EnsureWindowsSystemDispatcherQueueController();
			var options = GetWindowOptions(key);
			// Preserve the original options' physical-pixel bounds. Reactor's WindowSpec
			// dimensions are DIPs, so set the native presenter constraints explicitly.
			Window.AppWindow.SetPresenter(options.PresenterKind);
			Window.AppWindow.Resize(options.DefaultSize);
			if (Window.AppWindow.Presenter is OverlappedPresenter presenter) {
				presenter.IsResizable = options.IsResizable;
				presenter.IsMinimizable = options.IsMinimizable;
				presenter.IsMaximizable = options.IsMaximizable;
				presenter.PreferredMinimumWidth = options.MinimumSize.Width;
				presenter.PreferredMinimumHeight = options.MinimumSize.Height;
				if (options.MaximumSize.Width > 0) presenter.PreferredMaximumWidth = options.MaximumSize.Width;
				if (options.MaximumSize.Height > 0) presenter.PreferredMaximumHeight = options.MaximumSize.Height;
			}
			if (options.DefaultPosition is { } position) Window.AppWindow.Move(position);
			Window.AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
			Window.AppWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
			Window.AppWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
			SetTheme();
			CreateAdapter();
			Window.Activated += OnActivated;
			Window.Closed += OnClosed;
			if (Window.Content is FrameworkElement root) root.ActualThemeChanged += OnThemeChanged;
		}
		internal void ValidateReload() {
			if (!Window.DispatcherQueue.HasThreadAccess) throw new InvalidOperationException("Reload must run on the UI thread.");
		}
		internal void RefreshText() {
			var options = GetWindowOptions(_windowKey);
			Window.Title = options.TitleProvider?.Invoke() ?? options.Title;
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
