using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Main;

// Locale participates in props equality so language changes refresh localized text.
internal sealed record StatusBarProps(
	bool Enabled,
	string? Failure,
	string ToggleHint,
	string Locale,
	Action<bool> OnEnabledChanged);

internal sealed class StatusBar : Component<StatusBarProps> {
	public override Element Render() =>
		SettingsCard(
			Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
				HStack(8,
					TextBlock(Localization.Get("MainWindow_Localized5.Text"))
						.VerticalAlignment(VerticalAlignment.Center),
					CompactToggle(Props.Enabled, Props.OnEnabledChanged, Localization.Get("Enableclicking"))
						.ToolTip(Props.ToggleHint)
				).Grid(0, 0),
				HStack(6,
					Ellipse().Width(8).Height(8)
						.Fill(ThemeResource.Brush(Props.Enabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush"))
						.VerticalAlignment(VerticalAlignment.Center),
					TextBlock(Props.Failure is null ? Localization.Get(Props.Enabled ? "Enabled" : "Disabled") : Localization.Get("Blocked"))
						.VerticalAlignment(VerticalAlignment.Center).ToolTip(Props.Failure ?? string.Empty)
				).Grid(0, 1)
			)
		).Background(new ThemeRef("LayerFillColorDefaultBrush")).Padding(new Thickness(12, 6, 12, 6));
}
