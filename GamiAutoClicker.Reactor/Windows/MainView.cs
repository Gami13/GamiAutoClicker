using GamiAutoClicker.Components.Main;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed partial class MainView : Component {
	public override Element Render() {
		var bindings = UseBindings();
		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Auto, GridSize.Star(), GridSize.Auto],
			Component<TimingCard, TimingCardProps>(bindings.Timing).Grid(0, 0),
			Component<InputCard, InputCardProps>(bindings.Input).Grid(1, 0),
			VStack(0).Grid(2, 0),
			Component<StatusBar, StatusBarProps>(bindings.Status).Grid(3, 0)
		).Margin(new Thickness(16, 8, 16, 12)).Set(grid => grid.RowSpacing = 8);
	}
}
