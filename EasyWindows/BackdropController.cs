using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;

using Windows.UI;
using WinRT;

namespace Gami;

public static partial class EasyWindows {
	public class BackdropController : IDisposable {

		private readonly Window _window;
		private readonly SystemBackdropConfiguration _configurationSource;
		private ISystemBackdropController? _controller;
		private bool _disposed;

		private Func<Color>? _getFallbackColor;
		private Func<Color>? _getTintColor;
		private Func<float>? _getTintOpacity;
		private Func<float>? _getLuminosityOpacity;
		private Action<Color>? _setFallbackColor;
		private Action<Color>? _setTintColor;
		private Action<float>? _setTintOpacity;
		private Action<float>? _setLuminosityOpacity;

		public BackdropController(Window window, SystemBackdropConfiguration configurationSource) {
			_window = window;
			_configurationSource = configurationSource;
			CreateController();
		}

		public void CreateController() {
			DisposeController();

			if (EasyWindows.Manager.ThemeSettings.type == ThemeType.Mica) CreateMicaController();
			else if (EasyWindows.Manager.ThemeSettings.type == ThemeType.Acrylic) CreateAcrylicController();
		}

		private void CreateMicaController() {
			var mc = new MicaController { Kind = EasyWindows.Manager.ThemeSettings.micaKind };
			_getFallbackColor = () => mc.FallbackColor; _setFallbackColor = v => mc.FallbackColor = v;
			_getTintColor = () => mc.TintColor; _setTintColor = v => mc.TintColor = v;
			_getTintOpacity = () => mc.TintOpacity; _setTintOpacity = v => mc.TintOpacity = v;
			_getLuminosityOpacity = () => mc.LuminosityOpacity; _setLuminosityOpacity = v => mc.LuminosityOpacity = v;
			mc.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
			mc.SetSystemBackdropConfiguration(_configurationSource);
			if (EasyWindows.Manager.ThemeSettings.shouldOverride) {
				mc.FallbackColor = EasyWindows.Manager.ThemeSettings.fallbackColor;
				mc.TintColor = EasyWindows.Manager.ThemeSettings.tintColor;
				mc.TintOpacity = EasyWindows.Manager.ThemeSettings.tintOpacity;
				mc.LuminosityOpacity = EasyWindows.Manager.ThemeSettings.luminosityOpacity;
			}
			_controller = mc;
		}

		private void CreateAcrylicController() {
			var ac = new DesktopAcrylicController { Kind = EasyWindows.Manager.ThemeSettings.acrylicKind };
			_getFallbackColor = () => ac.FallbackColor; _setFallbackColor = v => ac.FallbackColor = v;
			_getTintColor = () => ac.TintColor; _setTintColor = v => ac.TintColor = v;
			_getTintOpacity = () => ac.TintOpacity; _setTintOpacity = v => ac.TintOpacity = v;
			_getLuminosityOpacity = () => ac.LuminosityOpacity; _setLuminosityOpacity = v => ac.LuminosityOpacity = v;
			ac.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
			ac.SetSystemBackdropConfiguration(_configurationSource);
			if (EasyWindows.Manager.ThemeSettings.shouldOverride) {
				ac.FallbackColor = EasyWindows.Manager.ThemeSettings.fallbackColor;
				ac.TintColor = EasyWindows.Manager.ThemeSettings.tintColor;
				ac.TintOpacity = EasyWindows.Manager.ThemeSettings.tintOpacity;
				ac.LuminosityOpacity = EasyWindows.Manager.ThemeSettings.luminosityOpacity;
			}
			_controller = ac;
		}

		public void SetMicaKind() {
			if (_controller is MicaController mc) mc.Kind = EasyWindows.Manager.ThemeSettings.micaKind;
		}
		public void SetAcrylicKind() {
			if (_controller is DesktopAcrylicController ac) ac.Kind = EasyWindows.Manager.ThemeSettings.acrylicKind;
		}
		public void SetFallbackColor() {
			if (EasyWindows.Manager.ThemeSettings.shouldOverride) _setFallbackColor?.Invoke(EasyWindows.Manager.ThemeSettings.fallbackColor);
		}
		public void SetTintColor() {
			if (EasyWindows.Manager.ThemeSettings.shouldOverride) _setTintColor?.Invoke(EasyWindows.Manager.ThemeSettings.tintColor);
		}
		public void SetLuminosityOpacity() {
			if (EasyWindows.Manager.ThemeSettings.shouldOverride) _setLuminosityOpacity?.Invoke(EasyWindows.Manager.ThemeSettings.luminosityOpacity);
		}

		public void SetTintOpacity() {
			if (!EasyWindows.Manager.ThemeSettings.shouldOverride) return;
			_setTintOpacity?.Invoke(EasyWindows.Manager.ThemeSettings.tintOpacity);
			// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
			// Slightly modify the tint color to force a visual update, as changing opacity alone doesn't always work
			if (_getTintColor == null || _setTintColor == null) return;
			var currentColor = _getTintColor();
			var tempColor = currentColor;
			tempColor.A = (byte)(currentColor.A < 255 ? currentColor.A + 1 : currentColor.A - 1);
			_setTintColor(tempColor);
			_setTintColor(currentColor);
		}

		public Color? GetFallbackColor() => _getFallbackColor?.Invoke();
		public Color? GetTintColor() => _getTintColor?.Invoke();
		public float? GetTintOpacity() => _getTintOpacity?.Invoke();
		public float? GetLuminosityOpacity() => _getLuminosityOpacity?.Invoke();

		private void DisposeController() {
			(_controller as IDisposable)?.Dispose();
			_controller = null;
			_getFallbackColor = _getTintColor = null;
			_setFallbackColor = _setTintColor = null;
			_getTintOpacity = _getLuminosityOpacity = null;
			_setTintOpacity = _setLuminosityOpacity = null;
		}

		public void Dispose() {
			if (_disposed) return;
			DisposeController();
			_disposed = true;
			GC.SuppressFinalize(this);
		}
	}
}