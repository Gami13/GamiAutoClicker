using GamiAutoClicker.Pages.Settings;
using Gami;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GamiAutoClicker;

internal enum SettingsSection {
	Appearance,
	General
}

internal sealed class SettingsWindowState {
	public SettingsSection SelectedSection { get; set; } = SettingsSection.Appearance;
	public bool IsPaneOpen { get; set; } = true;
}

internal sealed partial class SettingsPage : Page, System.IDisposable {
	private readonly SettingsWindowState _state = EasyWindows.Windows[WindowKey.Settings].GetState<SettingsWindowState>();
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private SplitView? _splitView;
	private long _displayModeCallbackToken;
	private long? _paneOpenCallbackToken;

	internal bool IsGeneralSelected => ReferenceEquals(SettingsNavigation.SelectedItem, GeneralNavigationItem);
	internal bool IsPaneOpen => SettingsNavigation.IsPaneOpen;

	public SettingsPage() {
		InitializeComponent();
		SettingsNavigation.IsPaneOpen = _state.IsPaneOpen;
		SettingsNavigation.SelectedItem = _state.SelectedSection == SettingsSection.General
			? GeneralNavigationItem
			: AppearanceNavigationItem;
		Unloaded += OnUnloaded;
	}

	public void Dispose() {
		SettingsNavigation.SelectionChanged -= OnNavigationSelectionChanged;
		DetachPaneStateCallback();
		GeneralSettings.EndEditing(this);
		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = null;
	}

	private void UpdateInputPause() {
		if (SettingsNavigation.IsLoaded && _state.SelectedSection == SettingsSection.General) GeneralSettings.BeginEditing(this);
		else GeneralSettings.EndEditing(this);
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs e) {
		// Restore the model after template initialization, before observing UI changes.
		DetachPaneStateCallback();
		SettingsNavigation.IsPaneOpen = _state.IsPaneOpen;
		_paneOpenCallbackToken = SettingsNavigation.RegisterPropertyChangedCallback(
			NavigationView.IsPaneOpenProperty, OnPaneOpenChanged);
		UpdateInputPause();
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

	private void OnPaneOpenChanged(DependencyObject sender, DependencyProperty property) {
		_state.IsPaneOpen = SettingsNavigation.IsPaneOpen;
	}

	private void OnUnloaded(object sender, RoutedEventArgs e) {
		DetachPaneStateCallback();
		GeneralSettings.EndEditing(this);
	}

	private void DetachPaneStateCallback() {
		if (_paneOpenCallbackToken is not { } token) return;
		SettingsNavigation.UnregisterPropertyChangedCallback(NavigationView.IsPaneOpenProperty, token);
		_paneOpenCallbackToken = null;
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
		if (!ReferenceEquals(args.SelectedItem, AppearanceNavigationItem)
			&& !ReferenceEquals(args.SelectedItem, GeneralNavigationItem)) return;
		_state.SelectedSection = ReferenceEquals(args.SelectedItem, AppearanceNavigationItem)
			? SettingsSection.Appearance
			: SettingsSection.General;

		SettingsContent.Content = _state.SelectedSection == SettingsSection.Appearance
			? _appearancePage
			: _generalPage;
		UpdateInputPause();
	}
}
