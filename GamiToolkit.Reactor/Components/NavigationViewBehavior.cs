using Microsoft.UI.Reactor;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Markup;
namespace GamiToolkit.Components;

// WinUI's Auto mode normally overlays the content in compact mode. The original
// settings window keeps the compact rail inline and puts Save in the footer menu.
public sealed class NavigationViewBehavior : IDisposable {
	private readonly NavigationView _view;
	private readonly NavigationViewItem _save = new() { SelectsOnInvoked = false, Icon = new SymbolIcon(Symbol.Save) };
	private SplitView? _split;
	private long _token;
	private Action? _saveAction;
	private readonly bool _retainedPaneOpen;
	private readonly Action _ready;
	private bool _restoredPane;

	public NavigationViewBehavior(NavigationView view, bool retainedPaneOpen, Action ready) {
		_retainedPaneOpen = retainedPaneOpen;
		_ready = ready;
		_view = view;
		view.Resources = ResourceStyles.NavigationPaneResources();
		view.FooterMenuItems.Add(_save);
		view.Loaded += Loaded; view.Unloaded += Unloaded; view.PaneOpening += PaneOpening;
		view.ItemInvoked += Invoked; view.ActualThemeChanged += ThemeChanged;

	}
	public void Update(string label, string hint, Action save) {
		_saveAction = save; _save.Content = label; ToolTipService.SetToolTip(_save, hint);
		Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(_save, label);
		if (_view.MenuItems.Count > 1 && _view.MenuItems[1] is NavigationViewItem appearance) appearance.Icon = new FontIcon { Glyph = "\uE790" };
		RefreshResources();
	}
	private void Loaded(object sender, RoutedEventArgs args) {
		var found = FindSplit(_view);
		if (!ReferenceEquals(_split, found)) {
			DetachSplit(); _split = found;
			if (_split is not null) _token = _split.RegisterPropertyChangedCallback(SplitView.DisplayModeProperty, (_, _) => Inline());
		}
		Inline();
		if (!_restoredPane) {
			_view.IsPaneOpen = _retainedPaneOpen;
			_restoredPane = true;
			_ready();
		}
	}
	private void PaneOpening(NavigationView sender, object args) => Inline();
	private void Inline() { if (_split?.DisplayMode == SplitViewDisplayMode.CompactOverlay) _split.DisplayMode = SplitViewDisplayMode.CompactInline; }
	private void Unloaded(object sender, RoutedEventArgs args) => DetachSplit();
	private void Invoked(NavigationView sender, NavigationViewItemInvokedEventArgs args) { if (ReferenceEquals(args.InvokedItemContainer, _save)) _saveAction?.Invoke(); }
	private void ThemeChanged(FrameworkElement sender, object args) => RefreshResources();
	private void RefreshResources() {
		foreach (string state in new[] { "", "PointerOver", "Pressed", "Disabled" }) {
			string suffix = state.Length == 0 ? "" : state;
			_save.Resources["NavigationViewItemBackground" + suffix] = ThemeResource.Brush("AccentButtonBackground" + suffix);
			_save.Resources["NavigationViewItemForeground" + suffix] = ThemeResource.Get<Brush>("AccentButtonForeground" + suffix, ThemeResource.Brush(state == "Disabled" ? "TextFillColorDisabledBrush" : "TextFillColorPrimaryBrush"));
		}
	}
	private void DetachSplit() { if (_split is not null) _split.UnregisterPropertyChangedCallback(SplitView.DisplayModeProperty, _token); _split = null; }
	private static SplitView? FindSplit(DependencyObject root) {
		if (root is SplitView split) return split;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) if (FindSplit(VisualTreeHelper.GetChild(root, i)) is { } found) return found;
		return null;
	}
	public void Dispose() {
		DetachSplit(); _view.Loaded -= Loaded; _view.Unloaded -= Unloaded; _view.PaneOpening -= PaneOpening; _view.ItemInvoked -= Invoked; _view.ActualThemeChanged -= ThemeChanged;
		_view.FooterMenuItems.Remove(_save); _saveAction = null;
	}
}
