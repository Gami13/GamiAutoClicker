using Gami;
using GamiAutoClicker.Components.Settings;
using GamiToolkit.Components;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed partial class AppearancePage : ResponsiveComponent {
	public override Element Render() {
		double width = UseContentWidth();
		bool wide = width >= SettingsLayout.TwoColumnWidth;
		var bindings = UseBindings(width >= SettingsLayout.TwoColorPickerWidth);
		var header = SettingsLayout.Header(
			title: Localization.Get("AppearancePage_Localized1.Text"),
			description: Localization.Get("AppearancePage_Localized2.Text"),
			restoreLabel: Localization.Get("Restoredefaults"),
			expanded: width >= SettingsLayout.RestoreLabelWidth,
			restore: EasyWindows.RestoreThemeDefaults);
		var cards = Grid(
			[GridSize.Star(), wide ? GridSize.Star() : GridSize.Px(0)],
			[GridSize.Auto, GridSize.Auto, GridSize.Auto],
			Component<AppearanceSummaryCard, AppearanceSummaryCardProps>(bindings.Summary).Grid(0, 0),
			Component<MaterialSettingsCard, MaterialSettingsCardProps>(bindings.Material).Grid(1, 0),
			Component<AdvancedTuningCard, AdvancedTuningCardProps>(bindings.Tuning)
				.Grid(row: wide ? 0 : 2, column: wide ? 1 : 0, rowSpan: wide ? 2 : 1, columnSpan: 1)
		).Set(grid => {
			grid.ColumnSpacing = 12;
			grid.RowSpacing = 12;
		});

		return SettingsLayout.Page(header, cards, CaptureRoot);
	}
}
