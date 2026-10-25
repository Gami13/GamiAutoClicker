using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Main;

// Locale participates in props equality so language changes refresh localized text.
internal sealed record TimingCardProps(
	double DelayMilliseconds,
	double OffsetMilliseconds,
	bool ControlsEnabled,
	string Locale,
	Action<double> OnDelayChanged,
	Action<double> OnCpsChanged,
	Action<double> OnOffsetChanged);

internal sealed class TimingCard : Component<TimingCardProps> {
	public override Element Render() =>
		SettingsCard(
			Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto, GridSize.Auto, GridSize.Auto],
				TextBlock(Localization.Get("MainWindow_Localized1.Text"))
					.VerticalAlignment(VerticalAlignment.Center).Grid(0, 0),
				UnitNumberBox(
					value: Props.DelayMilliseconds,
					changed: Props.OnDelayChanged,
					unit: "ms",
					decimals: 0,
					smallChange: 1,
					minimum: ClickEngine.MinimumIntervalMilliseconds,
					maximum: 1_000_000,
					label: Localization.Get("MainWindow_Localized1.Text"),
					enabled: Props.ControlsEnabled
				).Grid(0, 1),
				TextBlock(Localization.Get("MainWindow_Localized2.Text"))
					.VerticalAlignment(VerticalAlignment.Center).Grid(1, 0),
				UnitNumberBox(
					value: 1000d / Props.DelayMilliseconds,
					changed: Props.OnCpsChanged,
					unit: "CPS",
					decimals: 3,
					smallChange: 0.001,
					minimum: 0.001,
					maximum: ClickEngine.MaximumClicksPerSecond,
					label: Localization.Get("MainWindow_Localized2.Text"),
					enabled: Props.ControlsEnabled
				).Grid(1, 1),
				TextBlock(Localization.Get("MainWindow_RandomOffset.Text"))
					.VerticalAlignment(VerticalAlignment.Center).Grid(2, 0),
				UnitNumberBox(
					value: Props.OffsetMilliseconds,
					changed: Props.OnOffsetChanged,
					unit: "ms",
					decimals: 0,
					smallChange: 1,
					minimum: 0,
					maximum: 1_000_000,
					label: Localization.Get("RandomOffset"),
					enabled: Props.ControlsEnabled
				).ToolTip(Localization.Get("RandomOffsetHint")).Grid(2, 1)
			).Set(grid => grid.RowSpacing = 8)
		).Padding(new Thickness(12, 8, 12, 8));
}
