using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using System;
using Windows.UI;
using WinRT;

namespace Gami;

internal interface IBackdropAdapter : IDisposable {
	Color FallbackColor { get; set; }
	Color TintColor { get; set; }
	float TintOpacity { get; set; }
	float LuminosityOpacity { get; set; }
}

internal static class BackdropHelper {
	internal static DesktopAcrylicController CreateAcrylicController(
		DesktopAcrylicKind kind,
		SystemBackdropConfiguration configuration,
		ICompositionSupportsSystemBackdrop target,
		EasyWindows.ThemeSettings theme) {
		var controller = new DesktopAcrylicController {
			Kind = kind
		};
		controller.SetSystemBackdropConfiguration(configuration);
		controller.AddSystemBackdropTarget(target);

		ApplyOverrides(controller, theme);

		return controller;
	}

	internal static void ApplyOverrides(MicaController controller, EasyWindows.ThemeSettings theme) {
		if (!theme.ShouldOverride) return;

		if (controller.FallbackColor != theme.FallbackColor) {
			controller.FallbackColor = theme.FallbackColor;
		}
		if (controller.TintColor != theme.TintColor) {
			controller.TintColor = theme.TintColor;
		}
		if (Math.Abs(controller.TintOpacity - theme.TintOpacity) > 0.001f) {
			controller.TintOpacity = theme.TintOpacity;
		}
		if (Math.Abs(controller.LuminosityOpacity - theme.LuminosityOpacity) > 0.001f) {
			controller.LuminosityOpacity = theme.LuminosityOpacity;
		}
	}

	internal static void ApplyOverrides(DesktopAcrylicController controller, EasyWindows.ThemeSettings theme) {
		if (!theme.ShouldOverride) return;

		if (controller.FallbackColor != theme.FallbackColor) {
			controller.FallbackColor = theme.FallbackColor;
		}
		if (controller.TintColor != theme.TintColor) {
			controller.TintColor = theme.TintColor;
		}
		if (Math.Abs(controller.TintOpacity - theme.TintOpacity) > 0.001f) {
			controller.TintOpacity = theme.TintOpacity;
			RefreshTint(controller.TintColor, value => controller.TintColor = value);
		}
		if (Math.Abs(controller.LuminosityOpacity - theme.LuminosityOpacity) > 0.001f) {
			controller.LuminosityOpacity = theme.LuminosityOpacity;
		}
	}

	internal static void RefreshTint(Color currentColor, Action<Color> setTintColor) {
		// Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10717
		var temporaryColor = currentColor;
		temporaryColor.A = (byte)(currentColor.A < 255 ? currentColor.A + 1 : currentColor.A - 1);
		setTintColor(temporaryColor);
		setTintColor(currentColor);
	}
}

internal sealed class MicaAdapter : IBackdropAdapter {
	private readonly MicaController _controller;

	public MicaAdapter(Window window, SystemBackdropConfiguration configurationSource, MicaKind kind) {
		_controller = new MicaController {
			Kind = kind
		};

		_controller.AddSystemBackdropTarget(window.As<ICompositionSupportsSystemBackdrop>());
		_controller.SetSystemBackdropConfiguration(configurationSource);
		BackdropHelper.ApplyOverrides(_controller, EasyWindows.Theme);
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
			if (!EasyWindows.Theme.ShouldOverride) return;

			_controller.TintOpacity = value;
			BackdropHelper.RefreshTint(_controller.TintColor, color => _controller.TintColor = color);
		}
	}

	public float LuminosityOpacity {
		get => _controller.LuminosityOpacity;
		set => _controller.LuminosityOpacity = value;
	}

	public void Dispose() => _controller.Dispose();
}

internal sealed class AcrylicAdapter : IBackdropAdapter {
	private readonly DesktopAcrylicController _controller;

	public AcrylicAdapter(Window window, SystemBackdropConfiguration configurationSource, DesktopAcrylicKind kind) {
		_controller = BackdropHelper.CreateAcrylicController(
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
			if (!EasyWindows.Theme.ShouldOverride) return;

			_controller.TintOpacity = value;
			BackdropHelper.RefreshTint(_controller.TintColor, color => _controller.TintColor = color);
		}
	}

	public float LuminosityOpacity {
		get => _controller.LuminosityOpacity;
		set => _controller.LuminosityOpacity = value;
	}

	public void Dispose() => _controller.Dispose();
}
