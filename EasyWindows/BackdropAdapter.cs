using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;
using WinRT;

namespace Gami;

public interface IBackdropAdapter : IDisposable {
	Color FallbackColor { get; set; }
	Color TintColor { get; set; }
	float TintOpacity { get; set; }
	float LuminosityOpacity { get; set; }
}

public class MicaAdapter : IBackdropAdapter {
	private readonly MicaController _controller;

	public MicaAdapter(Window window, SystemBackdropConfiguration configurationSource, MicaKind kind) {
		_controller = new MicaController {
			Kind = kind
		};

		_controller.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		_controller.SetSystemBackdropConfiguration(configurationSource);
		ApplyOverrides();
	}

	public Color FallbackColor {
		get => _controller.FallbackColor;
		set => _controller.FallbackColor = value;
	}

	public Color TintColor {
		get => _controller.TintColor;
		set => _controller.TintColor = value;
	}

	public float TintOpacity {
		get => _controller.TintOpacity;
		set {
			if (!EasyWindows.Theme.shouldOverride) return;

			_controller.TintOpacity = value;
			// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
			// Slightly modify the tint color to force a visual update, as changing opacity alone doesn't always work
			var currentColor = _controller.TintColor;
			var tempColor = currentColor;
			tempColor.A = (byte)(currentColor.A < 255 ? currentColor.A + 1 : currentColor.A - 1);
			_controller.TintColor = tempColor;
			_controller.TintColor = currentColor;
		}
	}

	public float LuminosityOpacity {
		get => _controller.LuminosityOpacity;
		set => _controller.LuminosityOpacity = value;
	}

	private void ApplyOverrides() {
		if (!EasyWindows.Theme.shouldOverride) return;

		var settings = EasyWindows.Theme;
		_controller.FallbackColor = settings.fallbackColor;
		_controller.TintColor = settings.tintColor;
		_controller.TintOpacity = settings.tintOpacity;
		_controller.LuminosityOpacity = settings.luminosityOpacity;
	}

	public void Dispose() => _controller.Dispose();
}

public class AcrylicAdapter : IBackdropAdapter {
	private readonly DesktopAcrylicController _controller;

	public AcrylicAdapter(Window window, SystemBackdropConfiguration configurationSource, DesktopAcrylicKind kind) {
		_controller = new DesktopAcrylicController {
			Kind = kind
		};

		_controller.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		_controller.SetSystemBackdropConfiguration(configurationSource);
		ApplyOverrides();
	}

	public Color FallbackColor {
		get => _controller.FallbackColor;
		set => _controller.FallbackColor = value;
	}

	public Color TintColor {
		get => _controller.TintColor;
		set => _controller.TintColor = value;
	}

	public float TintOpacity {
		get => _controller.TintOpacity;
		set {
			if (!EasyWindows.Theme.shouldOverride) return;

			_controller.TintOpacity = value;
			// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
			// Slightly modify the tint color to force a visual update, as changing opacity alone doesn't always work
			var currentColor = _controller.TintColor;
			var tempColor = currentColor;
			tempColor.A = (byte)(currentColor.A < 255 ? currentColor.A + 1 : currentColor.A - 1);
			_controller.TintColor = tempColor;
			_controller.TintColor = currentColor;
		}
	}

	public float LuminosityOpacity {
		get => _controller.LuminosityOpacity;
		set => _controller.LuminosityOpacity = value;
	}

	private void ApplyOverrides() {
		if (!EasyWindows.Theme.shouldOverride) return;

		var settings = EasyWindows.Theme;
		_controller.FallbackColor = settings.fallbackColor;
		_controller.TintColor = settings.tintColor;
		_controller.TintOpacity = settings.tintOpacity;
		_controller.LuminosityOpacity = settings.luminosityOpacity;
	}

	public void Dispose() => _controller.Dispose();
}
