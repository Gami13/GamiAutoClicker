using GamiAutoClicker.Components.Settings;
using GamiToolkit.Components;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed partial class GeneralPage : ResponsiveComponent {
	public override Element Render() {
		double width = UseContentWidth();
		var bindings = UseBindings();
		bool wide = width >= SettingsLayout.TwoColumnWidth;

		var header = SettingsLayout.Header(
			title: Localization.Get("GeneralPage_Localized1.Text"),
			description: Localization.Get("GeneralPage_Localized2.Text"),
			restoreLabel: Localization.Get("Restoredefaults"),
			expanded: width >= SettingsLayout.RestoreLabelWidth,
			restore: bindings.RestoreDefaults);
		Element? error = bindings.Error is null ? null : InfoBar(null, bindings.Error) with {
			Key = "validation-" + bindings.Attempt,
			Severity = InfoBarSeverity.Error
		};
		var cards = Grid(
			[GridSize.Star(), wide ? GridSize.Star() : GridSize.Px(0)],
			[GridSize.Auto, GridSize.Auto],
			Component<KeyBindingsCard, KeyBindingsCardProps>(bindings.Keys).Grid(0, 0),
			Component<LanguageCard, LanguageCardProps>(bindings.Language).Grid(wide ? 0 : 1, wide ? 1 : 0)
		).Set(grid => {
			grid.ColumnSpacing = 12;
			grid.RowSpacing = 12;
		});

		return SettingsLayout.Page(header, VStack(12, error, cards), CaptureRoot);
	}
}
