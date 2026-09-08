using GamiAutoClicker.Pages.Settings;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;

namespace GamiAutoClicker;

internal sealed partial class SettingsWindow : Window {
	private readonly AppearancePage _appearancePage = new();
	private readonly GeneralPage _generalPage = new();
	private FrameworkElement? _navigationContent;

	public SettingsWindow() {
		InitializeComponent();
		SettingsNavigation.SelectedItem = AppearanceNavigationItem;
		UpdateContentMargin(SettingsNavigation.DisplayMode);
	}

	private void OnNavigationDisplayModeChanged(NavigationView sender, NavigationViewDisplayModeChangedEventArgs args) {
		UpdateContentMargin(args.DisplayMode);
		UpdateOverlayClip(sender.IsPaneOpen);
	}

	private void OnNavigationLoaded(object sender, RoutedEventArgs args) {
		// Clip the complete content surface, including NavigationView's page background.
		_navigationContent = FindSplitView(SettingsNavigation)?.Content as FrameworkElement;
		UpdateOverlayClip(SettingsNavigation.IsPaneOpen);
	}

	private static SplitView? FindSplitView(DependencyObject root) {
		if (root is SplitView splitView) return splitView;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) {
			if (FindSplitView(VisualTreeHelper.GetChild(root, i)) is { } result) return result;
		}
		return null;
	}

	private void OnNavigationSizeChanged(object sender, SizeChangedEventArgs args) {
		UpdateOverlayClip(SettingsNavigation.IsPaneOpen);
	}

	private void OnPaneOpening(NavigationView sender, object args) => UpdateOverlayClip(true);

	private void OnPaneClosed(NavigationView sender, object args) => UpdateOverlayClip(false);

	private void UpdateOverlayClip(bool isPaneOpen) {
		if (_navigationContent is null) return;
		if (!isPaneOpen || SettingsNavigation.DisplayMode == NavigationViewDisplayMode.Expanded) {
			_navigationContent.Clip = null;
			return;
		}

		double contentLeft = _navigationContent.TransformToVisual(SettingsNavigation).TransformPoint(new Point()).X;
		double coveredWidth = Math.Clamp(SettingsNavigation.OpenPaneLength - contentLeft, 0, _navigationContent.ActualWidth);
		_navigationContent.Clip = new RectangleGeometry {
			Rect = new Rect(coveredWidth, 0, Math.Max(0, _navigationContent.ActualWidth - coveredWidth), _navigationContent.ActualHeight)
		};
	}

	private void UpdateContentMargin(NavigationViewDisplayMode displayMode) {
		if (SettingsContent is null) return;

		// Minimal navigation overlays its menu button on the content's top-left corner.
		SettingsContent.Margin = displayMode == NavigationViewDisplayMode.Minimal
			? new Thickness(0, 24, 0, 0)
			: new Thickness(0);
	}

	private void OnNavigationSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args) {
		if (SettingsContent is null) return;

		SettingsContent.Content = ReferenceEquals(args.SelectedItem, AppearanceNavigationItem)
			? _appearancePage
			: _generalPage;

		if (sender.DisplayMode != NavigationViewDisplayMode.Expanded) {
			sender.IsPaneOpen = false;
		}
	}
}
