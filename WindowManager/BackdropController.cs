using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;

using Windows.UI;
using WinRT;

namespace GamiAutoClicker.WindowManager;

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

		if (WindowController.ThemeSettings.type == ThemeType.Mica) CreateMicaController();
		else if (WindowController.ThemeSettings.type == ThemeType.Acrylic) CreateAcrylicController();
	}

	private void CreateMicaController() {
		var mc = new MicaController { Kind = WindowController.ThemeSettings.micaKind };
		_getFallbackColor = () => mc.FallbackColor; _setFallbackColor = v => mc.FallbackColor = v;
		_getTintColor = () => mc.TintColor; _setTintColor = v => mc.TintColor = v;
		_getTintOpacity = () => mc.TintOpacity; _setTintOpacity = v => mc.TintOpacity = v;
		_getLuminosityOpacity = () => mc.LuminosityOpacity; _setLuminosityOpacity = v => mc.LuminosityOpacity = v;
		mc.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
		mc.SetSystemBackdropConfiguration(_configurationSource);
		if (WindowController.ThemeSettings.shouldOverride) {
			mc.FallbackColor = WindowController.ThemeSettings.fallbackColor;
			mc.TintColor = WindowController.ThemeSettings.tintColor;
			mc.TintOpacity = WindowController.ThemeSettings.tintOpacity;
			mc.LuminosityOpacity = WindowController.ThemeSettings.luminosityOpacity;
		}
		_controller = mc;
	}

	private void CreateAcrylicController() {
		var ac = new DesktopAcrylicController { Kind = WindowController.ThemeSettings.acrylicKind };
		_getFallbackColor = () => ac.FallbackColor; _setFallbackColor = v => ac.FallbackColor = v;
		_getTintColor = () => ac.TintColor; _setTintColor = v => ac.TintColor = v;
		_getTintOpacity = () => ac.TintOpacity; _setTintOpacity = v => ac.TintOpacity = v;
		_getLuminosityOpacity = () => ac.LuminosityOpacity; _setLuminosityOpacity = v => ac.LuminosityOpacity = v;
		ac.AddSystemBackdropTarget(_window.As<ICompositionSupportsSystemBackdrop>());
		ac.SetSystemBackdropConfiguration(_configurationSource);
		if (WindowController.ThemeSettings.shouldOverride) {
			ac.FallbackColor = WindowController.ThemeSettings.fallbackColor;
			ac.TintColor = WindowController.ThemeSettings.tintColor;
			ac.TintOpacity = WindowController.ThemeSettings.tintOpacity;
			ac.LuminosityOpacity = WindowController.ThemeSettings.luminosityOpacity;
		}
		_controller = ac;
	}

	public void SetMicaKind() {
		if (_controller is MicaController mc) mc.Kind = WindowController.ThemeSettings.micaKind;
	}
	public void SetAcrylicKind() {
		if (_controller is DesktopAcrylicController ac) ac.Kind = WindowController.ThemeSettings.acrylicKind;
	}
	public void SetFallbackColor() {
		if (WindowController.ThemeSettings.shouldOverride) _setFallbackColor?.Invoke(WindowController.ThemeSettings.fallbackColor);
	}
	public void SetTintColor() {
		if (WindowController.ThemeSettings.shouldOverride) _setTintColor?.Invoke(WindowController.ThemeSettings.tintColor);
	}
	public void SetLuminosityOpacity() {
		if (WindowController.ThemeSettings.shouldOverride) _setLuminosityOpacity?.Invoke(WindowController.ThemeSettings.luminosityOpacity);
	}

	public void SetTintOpacity() {
		if (!WindowController.ThemeSettings.shouldOverride) return;
		_setTintOpacity?.Invoke(WindowController.ThemeSettings.tintOpacity);
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
