using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Gami;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "Will be extracted to separate package")]
public partial class EasyWindowsBackdrop : SystemBackdrop {
	private sealed class TargetState(
		DesktopAcrylicController controller,
		SystemBackdropConfiguration configuration,
		DesktopAcrylicKind kind,
		bool hasAppliedOverrides) : IDisposable {
		public DesktopAcrylicController Controller { get; set; } = controller;
		public SystemBackdropConfiguration Configuration { get; set; } = configuration;
		public DesktopAcrylicKind Kind { get; set; } = kind;
		public bool HasAppliedOverrides { get; set; } = hasAppliedOverrides;

		public void Dispose() {
			Controller.Dispose();
		}
	}

	private readonly Dictionary<ICompositionSupportsSystemBackdrop, TargetState> _targets = new();

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Disposed via TargetState in OnTargetDisconnected/Dispose")]
	protected override void OnTargetConnected(ICompositionSupportsSystemBackdrop connectedTarget, XamlRoot xamlRoot) {
		base.OnTargetConnected(connectedTarget, xamlRoot);

		if (!DesktopAcrylicController.IsSupported()) {
			return;
		}

		var config = GetDefaultSystemBackdropConfiguration(connectedTarget, xamlRoot);
		UpdateConfigurationTheme(config, EasyWindows.Theme);

		var kind = GetFlyoutAcrylicKind(EasyWindows.Theme);
		var controller = BackdropHelper.CreateAcrylicController(kind, config, connectedTarget, EasyWindows.Theme);

		_targets[connectedTarget] = new TargetState(controller, config, kind, EasyWindows.Theme.shouldOverride);

		if (_targets.Count == 1) {
			EasyWindows.ThemeChanged += OnThemeChanged;
		}
	}

	protected override void OnTargetDisconnected(ICompositionSupportsSystemBackdrop disconnectedTarget) {
		base.OnTargetDisconnected(disconnectedTarget);

		if (_targets.Remove(disconnectedTarget, out var state)) {
			state.Controller.RemoveSystemBackdropTarget(disconnectedTarget);
			state.Dispose();
		}

		if (_targets.Count == 0) {
			EasyWindows.ThemeChanged -= OnThemeChanged;
		}
	}

	protected override void OnDefaultSystemBackdropConfigurationChanged(ICompositionSupportsSystemBackdrop target, XamlRoot xamlRoot) {
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
		var theme = EasyWindows.Theme;
		var requiredKind = GetFlyoutAcrylicKind(theme);

		foreach (var (target, state) in _targets) {
			UpdateConfigurationTheme(state.Configuration, theme);
			state.Controller.SetSystemBackdropConfiguration(state.Configuration);

			if (state.Kind != requiredKind || (state.HasAppliedOverrides && !theme.shouldOverride)) {
				state.Controller.RemoveSystemBackdropTarget(target);
				state.Controller.Dispose();

				var newController = BackdropHelper.CreateAcrylicController(requiredKind, state.Configuration, target, theme);
				state.Controller = newController;
				state.Kind = requiredKind;
				state.HasAppliedOverrides = theme.shouldOverride;
			}
			else if (theme.shouldOverride) {
				BackdropHelper.ApplyOverrides(state.Controller, theme);
				state.HasAppliedOverrides = true;
			}
		}
	}

	private static void UpdateConfigurationTheme(SystemBackdropConfiguration config, EasyWindows.ThemeSettings theme) =>
		config.Theme = theme.theme;

	private static DesktopAcrylicKind GetFlyoutAcrylicKind(EasyWindows.ThemeSettings theme) =>
		theme.backdropMaterial == EasyWindows.BackdropMaterial.AcrylicThin
			? DesktopAcrylicKind.Thin
			: DesktopAcrylicKind.Base;
}
