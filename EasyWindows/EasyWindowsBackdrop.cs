using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gami;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
/// <summary>
/// Acrylic backdrop used by flyouts and other XAML elements that expose a
/// <see cref="SystemBackdrop"/> property. Unlike a window adapter, this
/// backdrop can be connected to several targets over its lifetime, so it
/// keeps one controller and configuration for each connected target.
/// </summary>
public partial class EasyWindowsBackdrop : SystemBackdrop {
	// SystemBackdrop targets can be connected and disconnected independently.
	// Keep the controller state per target so one flyout cannot affect another.
	private sealed class TargetState(
		DesktopAcrylicController controller,
		SystemBackdropConfiguration configuration,
		DesktopAcrylicKind kind) {
		public DesktopAcrylicController Controller { get; set; } = controller;
		public SystemBackdropConfiguration Configuration { get; set; } = configuration;
		public DesktopAcrylicKind Kind { get; set; } = kind;
	}

	private readonly Dictionary<ICompositionSupportsSystemBackdrop, TargetState> _targets = new();

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed via TargetState in OnTargetDisconnected/Dispose")]
	protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot) {
		// WinUI calls this when a flyout attaches this backdrop to a visual target.
		base.OnTargetConnected(connectedTarget, xamlRoot);

		if (!DesktopAcrylicController.IsSupported()) {
			return;
		}

		// Each target receives its own configuration and Acrylic controller.
		var config = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
		UpdateConfigurationTheme(config, EasyWindows.Theme);

		var kind = GetFlyoutAcrylicKind(EasyWindows.Theme);
		var controller = BackdropHelper.CreateAcrylicController(kind, config, connectedTarget, EasyWindows.Theme);

		_targets[connectedTarget] = new TargetState(controller, config, kind);

		if (_targets.Count == 1) {
			// Subscribe only while at least one target exists; this prevents this
			// backdrop instance from retaining an unused global event subscription.
			EasyWindows.ThemeChanged += OnThemeChanged;
		}
	}

	protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget) {
		// Remove the target before disposing its controller so the controller no
		// longer references the disconnected XAML object.
		base.OnTargetDisconnected(disconnectedTarget);

		if (_targets.Remove(disconnectedTarget, out var state)) {
			state.Controller.RemoveSystemBackdropTarget(disconnectedTarget);
			state.Controller.Dispose();
		}

		if (_targets.Count == 0) {
			EasyWindows.ThemeChanged -= OnThemeChanged;
		}
	}

	protected override void OnDefaultSystemBackdropConfigurationChanged(ICompositionSupportsSystemBackdrop target, XamlRoot xamlRoot) {
		// DPI, theme, or XAML-root changes can invalidate the default configuration
		// supplied by WinUI. Refresh only the affected target's configuration.
		base.OnDefaultSystemBackdropConfigurationChanged(target, xamlRoot);

		if (_targets.TryGetValue(target, out var state)) {
			var newConfig = GetDefaultSystemBackdropConfiguration(target, xamlRoot);
			UpdateConfigurationTheme(newConfig, EasyWindows.Theme);
			state.Configuration = newConfig;
			state.Controller.SetSystemBackdropConfiguration(newConfig);
		}
	}

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed via TargetState in OnTargetDisconnected/Dispose")]
	private void OnThemeChanged(object? sender, EventArgs e) {
		// EasyWindows owns the global settings. Existing flyout targets either get
		// their current values applied in place or receive a new controller when
		// the Acrylic kind or override state requires one.
		var theme = EasyWindows.Theme;
		var requiredKind = GetFlyoutAcrylicKind(theme);

		foreach (var (target, state) in _targets) {
			UpdateConfigurationTheme(state.Configuration, theme);
			state.Controller.SetSystemBackdropConfiguration(state.Configuration);

			if (state.Kind != requiredKind) {
				state.Controller.RemoveSystemBackdropTarget(target);
				state.Controller.Dispose();

				var newController = BackdropHelper.CreateAcrylicController(requiredKind, state.Configuration, target, theme);
				state.Controller = newController;
				state.Kind = requiredKind;
			}
			else if (theme.shouldOverride) {
				BackdropHelper.ApplyOverrides(state.Controller, theme);
			}
			else {
				state.Controller.ResetProperties();
			}
		}
	}

	private static void UpdateConfigurationTheme(SystemBackdropConfiguration config, EasyWindows.ThemeSettings theme) =>
		config.Theme = theme.theme;

	// Flyouts use Acrylic even when windows use Mica. Only the Acrylic variant
	// (Base versus Thin) is selected from the global backdrop setting.
	private static DesktopAcrylicKind GetFlyoutAcrylicKind(EasyWindows.ThemeSettings theme) =>
		theme.backdropMaterial == EasyWindows.BackdropMaterial.AcrylicThin
			? DesktopAcrylicKind.Thin
			: DesktopAcrylicKind.Base;
}
