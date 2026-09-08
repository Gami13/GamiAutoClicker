using GamiAutoClicker.Pages.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace GamiAutoClicker;

internal sealed partial class SettingsWindow : Window
{
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private SplitView? _splitView;
	private long _displayModeCallbackToken;

	public SettingsWindow()
	{
		InitializeComponent();
        Closed += (_, _) => GeneralSettings.IsEditing = false;
		SettingsNavigation.SelectedItem = AppearanceNavigationItem;
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs e)
	{
		var splitView = FindSplitView(SettingsNavigation);
		if (ReferenceEquals(_splitView, splitView)) return;

		_splitView?.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _displayModeCallbackToken);
		_splitView = splitView;
		if (_splitView is not null)
		{
			// Keep the compact pane pushing content aside instead of opening over it.
			// NavigationView resets its internal SplitView mode as the window adapts.
			EnsureCompactInlineMode(_splitView);
			_displayModeCallbackToken = _splitView.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (s, dp) =>
			{
				if (s is SplitView sv)
				{
					EnsureCompactInlineMode(sv);
				}
			});
		}
	}

	private void OnPaneOpening(NavigationView sender, object args)
	{
		if (_splitView is not null)
		{
			EnsureCompactInlineMode(_splitView);
		}
	}

	private static void EnsureCompactInlineMode(SplitView splitView)
	{
		if (splitView.DisplayMode == SplitViewDisplayMode.CompactOverlay)
		{
			splitView.DisplayMode = SplitViewDisplayMode.CompactInline;
		}
	}

	private static SplitView? FindSplitView(DependencyObject root)
	{
		if (root is SplitView splitView) return splitView;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
		{
			if (FindSplitView(VisualTreeHelper.GetChild(root, i)) is { } result) return result;
		}
		return null;
	}

	private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
	{
		if (SettingsContent is null) return;

		SettingsContent.Content = ReferenceEquals(args.SelectedItem, AppearanceNavigationItem)
			? _appearancePage
			: _generalPage;
	}
}
