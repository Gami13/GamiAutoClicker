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

internal static class AcrylicControllerHelper {
	public static DesktopAcrylicController Create(
		DesktopAcrylicKind kind,
		SystemBackdropConfiguration configuration,
		ICompositionSupportsSystemBackdrop target,
		EasyWindows.ThemeSettings theme) {
		var controller = new DesktopAcrylicController {
			Kind = kind
		};
		controller.SetSystemBackdropConfiguration(configuration);
		controller.AddSystemBackdropTarget(target);

		if (theme.shouldOverride) {
			ApplyOverrides(controller, theme);
		}

		return controller;
	}

	//guarding for less gpu composition calls
	public static void ApplyOverrides(DesktopAcrylicController controller, EasyWindows.ThemeSettings theme) {
		if (controller.FallbackColor != theme.fallbackColor) {
			controller.FallbackColor = theme.fallbackColor;
		}
		if (controller.TintColor != theme.tintColor) {
			controller.TintColor = theme.tintColor;
		}
		if (Math.Abs(controller.TintOpacity - theme.tintOpacity) > 0.001f) {
			SetTintOpacity(controller, theme.tintOpacity);
		}
		if (Math.Abs(controller.LuminosityOpacity - theme.luminosityOpacity) > 0.001f) {
			controller.LuminosityOpacity = theme.luminosityOpacity;
		}
	}

	public static void SetTintOpacity(DesktopAcrylicController controller, float opacity) {
		controller.TintOpacity = opacity;
		RefreshTint(controller);
	}

	private static void RefreshTint(DesktopAcrylicController controller) {
		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
		var currentColor = controller.TintColor;
		var temporaryColor = currentColor;
		temporaryColor.A = (byte)(currentColor.A < 255 ? currentColor.A + 1 : currentColor.A - 1);
		controller.TintColor = temporaryColor;
		controller.TintColor = currentColor;
	}
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
		_controller = AcrylicControllerHelper.Create(
			kind,
			configurationSource,
			window.As<ICompositionSupportsSystemBackdrop>(),
			EasyWindows.Theme);
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

			AcrylicControllerHelper.SetTintOpacity(_controller, value);
		}
	}

	public float LuminosityOpacity {
		get => _controller.LuminosityOpacity;
		set => _controller.LuminosityOpacity = value;
	}

	public void Dispose() => _controller.Dispose();
}
