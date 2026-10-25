using Microsoft.UI.Input;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using Windows.Graphics;
using static Microsoft.UI.Reactor.Factories;
namespace Gami;

internal static class WindowTitleBar {
	internal static Element Create(ReactorWindow window, object key, bool active) {
		var options = EasyWindows.GetWindowOptions(key);
		var action = options.Button;
		var button = action is null ? null : Button("", action.Action).Width(32).Height(28)
			.Foreground(new ThemeRef(active ? "WindowCaptionForeground" : "WindowCaptionForegroundDisabled")).Padding(1).Margin(new Thickness(-1.5, 0, 0, 0)).Background(new ThemeRef("ControlFillColorTransparentBrush")).BorderBrush(new SolidColorBrush(Microsoft.UI.Colors.Transparent)).CornerRadius(6)
			.AutomationName(action.AccessibleNameProvider?.Invoke() ?? "")
			.ToolTip(action.AccessibleNameProvider?.Invoke() ?? "")
			.Set(b => {
				b.Content = new SymbolIcon(action.Icon);

			}).Grid(0, 1);
		return Grid([GridSize.Px(0), GridSize.Auto, GridSize.Auto, GridSize.Star(), GridSize.Px(0)], [GridSize.Auto],
			button,
			TextBlock(options.TitleProvider?.Invoke() ?? options.Title)
				.Set(text => ApplyCaptionStyle(text, active))
				.Padding(new Thickness(4, 0, 0, 0)).VerticalAlignment(VerticalAlignment.Center).Grid(0, 2)
		).Height(32).Set(grid => {
			grid.ColumnSpacing = 2;
			if (grid.Tag is TitleBarState) return;
			grid.Tag = new TitleBarState(grid, window, action is not null);
		}).OnUnmountAdd(element => { if (element.Tag is TitleBarState state) { state.Dispose(); element.Tag = null; } });
	}


	private static Style? _activeCaptionStyle;
	private static Style? _inactiveCaptionStyle;

	private static void ApplyCaptionStyle(Microsoft.UI.Xaml.Controls.TextBlock text, bool active) {
		// Reactor supplies text defaults as local values. Remove those so the same
		// native caption style used by the original XAML controls all typography.
		foreach (var property in new[] {
			Microsoft.UI.Xaml.Controls.TextBlock.FontFamilyProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.FontSizeProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.FontWeightProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.FontStretchProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.FontStyleProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.CharacterSpacingProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.TextLineBoundsProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.LineHeightProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.LineStackingStrategyProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.OpticalMarginAlignmentProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.TextWrappingProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.TextTrimmingProperty,
			Microsoft.UI.Xaml.Controls.TextBlock.ForegroundProperty
		}) text.ClearValue(property);
		text.Style = active
			? _activeCaptionStyle ??= CreateCaptionStyle("WindowCaptionForeground")
			: _inactiveCaptionStyle ??= CreateCaptionStyle("WindowCaptionForegroundDisabled");
	}

	private static Style CreateCaptionStyle(string foregroundResource) => (Style)XamlReader.Load(
		"<Style xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" TargetType=\"TextBlock\" BasedOn=\"{StaticResource CaptionTextBlockStyle}\">" +
		"<Setter Property=\"Foreground\" Value=\"{ThemeResource " + foregroundResource + "}\" />" +
		"</Style>");

	private sealed class TitleBarState : IDisposable {
		private readonly Microsoft.UI.Xaml.Controls.Grid _grid;
		private readonly ReactorWindow _window;
		private readonly bool _hasButton;
		internal TitleBarState(Microsoft.UI.Xaml.Controls.Grid grid, ReactorWindow window, bool hasButton) {
			_grid = grid; _window = window; _hasButton = hasButton;
			grid.Loaded += Loaded;
			grid.SizeChanged += SizeChanged;

		}
		public void Dispose() { _grid.Loaded -= Loaded; _grid.SizeChanged -= SizeChanged; }
		private void Loaded(object sender, RoutedEventArgs args) {
			_window.NativeWindow.SetTitleBar(_grid);

			UpdateRegions();
		}

		private void SizeChanged(object sender, SizeChangedEventArgs args) => UpdateRegions();
		private void UpdateRegions() {
			if (_grid.XamlRoot is not { } root) return;
			double scale = root.RasterizationScale;
			_grid.ColumnDefinitions[0].Width = new GridLength(_window.AppWindow.TitleBar.LeftInset / scale);
			_grid.ColumnDefinitions[4].Width = new GridLength(_window.AppWindow.TitleBar.RightInset / scale);
			if (!_hasButton || _grid.Children.OfType<Microsoft.UI.Xaml.Controls.Button>().FirstOrDefault() is not { } button) return;
			Rect bounds = button.TransformToVisual(null).TransformBounds(new Rect(0, 0, button.ActualWidth, button.ActualHeight));
			InputNonClientPointerSource.GetForWindowId(_window.AppWindow.Id).SetRegionRects(NonClientRegionKind.Passthrough,
				[new RectInt32((int)Math.Round(bounds.X * scale), (int)Math.Round(bounds.Y * scale), (int)Math.Round(bounds.Width * scale), (int)Math.Round(bounds.Height * scale))]);
		}
	}
}
