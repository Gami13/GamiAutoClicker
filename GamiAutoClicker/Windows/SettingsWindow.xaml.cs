using GamiAutoClicker.Pages.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.ComponentModel;

namespace GamiAutoClicker;

internal enum SettingsSection {
	Appearance,
	General
}

internal sealed class SettingsWindowState : INotifyPropertyChanged {
	public event PropertyChangedEventHandler? PropertyChanged;

	public SettingsSection SelectedSection {
		get;
		set {
			if (field == value) return;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSection)));
		}
	} = SettingsSection.Appearance;

	public bool IsPaneOpen {
		get;
		set {
			if (field == value) return;
			field = value;
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPaneOpen)));
		}
	} = true;
}

internal sealed partial class SettingsPage : Page, System.IDisposable {
	internal SettingsWindowState State { get; }
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private SplitView? _splitView;
	private long _displayModeCallbackToken;

	public SettingsPage(SettingsWindowState state) {
		State = state;
		InitializeComponent();
	}

	private void OnNavigationItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) {
		if (ReferenceEquals(args.InvokedItemContainer, SaveButton)) SaveSettings();
	}

	private void SaveSettings() {
		SaveError.IsOpen = !SettingsStore.TrySave(out string error);
		SaveError.Message = error;
	}

	public void Dispose() {
		SettingsNavigation.Loaded -= OnNavigationLoaded;
		SettingsNavigation.ItemInvoked -= OnNavigationItemInvoked;
		SettingsNavigation.SelectionChanged -= OnNavigationSelectionChanged;
		Bindings.StopTracking();
		GeneralSettings.EndEditing(this);
		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = null;
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs e) {
		// Restore retained values after NavigationView's adaptive template initialization.
		Bindings.Update();
		UpdateInputPause();
		AttachSplitViewCallback();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e) {
		Bindings.StopTracking();
		GeneralSettings.EndEditing(this);
	}

	private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) => UpdateInputPause();

	private void UpdateInputPause() {
		if (SettingsNavigation.IsLoaded && ReferenceEquals(SettingsNavigation.SelectedItem, GeneralNavigationItem)) GeneralSettings.BeginEditing(this);
		else GeneralSettings.EndEditing(this);
	}

	private NavigationViewItem GetNavigationItem(SettingsSection section) =>
		section == SettingsSection.General ? GeneralNavigationItem : AppearanceNavigationItem;

	private Page GetSectionPage(SettingsSection section) =>
		section == SettingsSection.General ? _generalPage : _appearancePage;

	private void UpdateSelectedSection(object item) {
		if (ReferenceEquals(item, GeneralNavigationItem)) State.SelectedSection = SettingsSection.General;
		else if (ReferenceEquals(item, AppearanceNavigationItem)) State.SelectedSection = SettingsSection.Appearance;
	}

	private void AttachSplitViewCallback() {
		SplitView? splitView = FindSplitView(SettingsNavigation);
		if (ReferenceEquals(_splitView, splitView)) return;

		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = splitView;
		if (_splitView is null) return;

		// Keep the compact pane pushing content aside as NavigationView adapts.
		EnsureCompactInlineMode(_splitView);
		_displayModeCallbackToken = _splitView.RegisterPropertyChangedCallback(
			SplitView.DisplayModeProperty, static (sender, _) => EnsureCompactInlineMode((SplitView)sender));
	}

	private void OnPaneOpening(NavigationView sender, object args) => EnsureCompactInlineMode(_splitView);

	private static void EnsureCompactInlineMode(SplitView? splitView) {
		if (splitView?.DisplayMode == SplitViewDisplayMode.CompactOverlay) {
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
}
