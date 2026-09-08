using GamiAutoClicker.Pages.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GamiAutoClicker;

internal sealed partial class SettingsWindow : Window {
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private SplitView? _splitView;

	public SettingsWindow() {
		InitializeComponent();
		SettingsNavigation.SelectedItem = AppearanceNavigationItem;
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs e) {
		_splitView = FindSplitView(SettingsNavigation);
		if (_splitView is not null) {
			EnsureCompactInlineMode(_splitView);
			_splitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (s, dp) => {
				if (s is SplitView sv) {
					EnsureCompactInlineMode(sv);
				}
			});
		}
	}

	private void OnPaneOpening(NavigationView sender, object args) {
		if (_splitView is not null) {
			EnsureCompactInlineMode(_splitView);
		}
	}

	private static void EnsureCompactInlineMode(SplitView splitView) {
		if (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay) {
			splitView.DisplayMode = SplitViewDisplayMode.CompactInline;
		}
	}

	private static SplitView? FindSplitView(DependencyObject root) {
		if (root is SplitView splitView) return splitView;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) {
			if (FindSplitView(VisualTreeHelper.GetChild(root, i)) is { } result) return result;
		}
		return null;
	}

	private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
		if (SettingsContent is null) return;

		SettingsContent.Content = ReferenceEquals(args.SelectedItem, AppearanceNavigationItem)
			? _appearancePage
			: _generalPage;
	}
}
