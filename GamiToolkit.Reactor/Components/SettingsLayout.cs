using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;

namespace GamiToolkit.Components;

public static class SettingsLayout {
	public const double RestoreLabelWidth = 470;
	public const double TwoColumnWidth = 590;
	public const double TwoColorPickerWidth = 820;

	public static Element Page(Element header, Element content, Action<Microsoft.UI.Xaml.Controls.Grid> captureRoot) =>
		Grid([GridSize.Star()], [GridSize.Auto, GridSize.Star()],
			header.Grid(0, 0), ScrollContent(content).Grid(1, 0)
		).Set(captureRoot);

	public static Element Header(string title, string description, string restoreLabel, bool expanded, Action restore) =>
		Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
			VStack(2, Subtitle(title), SettingsCaption(description)).Grid(0, 0),
			RestoreDefaultsButton(restoreLabel, expanded, restore).Grid(0, 1)
		).Margin(new Thickness(24, 16, 24, 12)).Set(grid => grid.ColumnSpacing = 16);

	private static ScrollViewElement ScrollContent(Element content) =>
		ScrollView(content.Margin(new Thickness(24, 0, 24, 8))).Set(view => {
			view.HorizontalScrollMode = ScrollingScrollMode.Disabled;
			view.HorizontalScrollBarVisibility = ScrollingScrollBarVisibility.Hidden;
			view.VerticalScrollBarVisibility = ScrollingScrollBarVisibility.Auto;
		});
}
