using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker.Components.Main;

// Locale participates in props equality so language changes refresh localized text.
internal sealed record InputCardProps(
	string[] MouseLabels,
	int SelectedButtonIndex,
	bool HoldMode,
	bool ControlsEnabled,
	string HoldHint,
	string Locale,
	Action<int> OnMouseButtonChanged,
	Action<bool> OnHoldModeChanged);

internal sealed class InputCard : Component<InputCardProps> {
	public override Element Render() {
		string[] mouseLabels = Props.MouseLabels;

		return SettingsCard(
			Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto, GridSize.Auto],
				TextBlock(Localization.Get("MainWindow_Localized3.Text"))
					.VerticalAlignment(VerticalAlignment.Center).Grid(0, 0),
				ComboBox(mouseLabels, Props.SelectedButtonIndex, Props.OnMouseButtonChanged)
					.Width(120).IsEnabled(Props.ControlsEnabled)
					.AutomationName(Localization.Get("Clicktarget")).Grid(0, 1),
				HStack(5,
					TextBlock(Localization.Get("MainWindow_Localized4.Text"))
						.VerticalAlignment(VerticalAlignment.Center),
					TextBlock("\uE946").FontFamily("Segoe Fluent Icons").FontSize(12)
						.Foreground(new ThemeRef("TextFillColorSecondaryBrush"))
						.VerticalAlignment(VerticalAlignment.Center)
				).ToolTip(Props.HoldHint).Grid(1, 0),
				CompactToggle(Props.HoldMode, Props.OnHoldModeChanged, Localization.Get("Holdmode"))
					.IsEnabled(Props.ControlsEnabled).Grid(1, 1)
			).Set(grid => grid.RowSpacing = 8)
		).Padding(new Thickness(12, 8, 12, 8));
	}
}
