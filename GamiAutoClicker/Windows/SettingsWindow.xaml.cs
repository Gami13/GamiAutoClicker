using GamiAutoClicker.Pages.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GamiAutoClicker;

internal sealed partial class SettingsPage : Page, System.IDisposable {
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private SplitView? _splitView;
	private long _displayModeCallbackToken;

	internal bool IsGeneralSelected => ReferenceEquals(SettingsNavigation.SelectedItem, GeneralNavigationItem);
	internal bool IsPaneOpen => SettingsNavigation.IsPaneOpen;

	public SettingsPage() {
		InitializeComponent();
		SettingsNavigation.IsPaneOpen = true;
		SettingsNavigation.SelectedItem = AppearanceNavigationItem;
		SettingsNavigation.Loaded += (_, _) => UpdateInputPause();
		Unloaded += (_, _) => GeneralSettings.EndEditing(this);
	}

	public void Dispose() {
		GeneralSettings.EndEditing(this);
		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = null;
	}

	private void UpdateInputPause() {
		if (SettingsNavigation.IsLoaded && IsGeneralSelected) GeneralSettings.BeginEditing(this);
		else GeneralSettings.EndEditing(this);
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs e) {
		SplitView? splitView = FindSplitView(SettingsNavigation);
		if (ReferenceEquals(_splitView, splitView)) return;

		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = splitView;
		if (_splitView is not null) {
			// Keep the compact pane pushing content aside instead of opening over it.
			// NavigationView resets its internal SplitView mode as the window adapts.
			EnsureCompactInlineMode(_splitView);
			_displayModeCallbackToken = _splitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (s, dp) => {
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
		UpdateInputPause();
	}
}
