using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml.Controls;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed partial class SettingsView : Component {
	public override Element Render() {
		var bindings = UseBindings();
		Element page = bindings.Section == SettingsSection.General
			? Component<GeneralPage>()
			: Component<AppearancePage>();
		Element? error = bindings.SaveError is null ? null : InfoBar(null, bindings.SaveError) with {
			Key = "save-" + bindings.SaveAttempt,
			Severity = InfoBarSeverity.Error
		};

		var content = Grid(
			[GridSize.Star()], [GridSize.Auto, GridSize.Star()],
			error?.Grid(0, 0),
			page.Grid(1, 0));
		var navigation = NavigationView([
			NavItem(Localization.Get("SettingsWindow_GeneralNavigationItem.Content"), "Setting", "general"),
			NavItem(Localization.Get("SettingsWindow_AppearanceNavigationItem.Content"), "Edit", "appearance")
		], content) with {
			SelectedTag = bindings.Section == SettingsSection.General ? "general" : "appearance",
			OnSelectedTagChanged = bindings.SelectSection,
			IsPaneOpen = bindings.PaneOpen,
			OnPaneOpenChanged = bindings.ChangePaneOpen,
			IsSettingsVisible = false,
			IsBackButtonVisible = NavigationViewBackButtonVisible.Collapsed,
			IsTitleBarAutoPaddingEnabled = false,
			OpenPaneLength = 150,
			CompactModeThresholdWidth = 0,
			ExpandedModeThresholdWidth = 750
		};
		return navigation.PaneTitle(Localization.Get("Settings"))
			.Set(bindings.ConfigureNavigation).OnUnmountAdd(bindings.ReleaseNavigation);
	}
}
