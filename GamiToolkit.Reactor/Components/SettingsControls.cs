using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Globalization.NumberFormatting;
using static Microsoft.UI.Reactor.Factories;
namespace GamiToolkit.Components;

public static class SettingsControls {
	public static TextBlockElement SettingsCaption(string text) => Caption(text).TextWrapping(TextWrapping.Wrap);
	public static BorderElement SettingsCard(params Element?[] content) => Border(VStack(10, content))
		.Padding(12).Background(new ThemeRef("CardBackgroundFillColorDefaultBrush"))
		.BorderBrush(new ThemeRef("CardStrokeColorDefaultBrush")).BorderThickness(1).CornerRadius(8).VerticalAlignment(VerticalAlignment.Top);

	public static Element RestoreDefaultsButton(string label, bool expanded, Action restore) =>
		Button("", restore).AutomationName(label).ToolTip(label).MinWidth(0).VerticalAlignment(VerticalAlignment.Center)
		.Width(expanded ? double.NaN : 38).Height(expanded ? double.NaN : 38).Padding(expanded ? new Thickness(11, 5, 11, 6) : new Thickness(0)).Set(button => {
			var icon = new Microsoft.UI.Xaml.Controls.FontIcon { Glyph = "\uE72C", FontSize = expanded ? 14 : 16 };
			var content = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
			content.Children.Add(icon);
			if (expanded) content.Children.Add(new Microsoft.UI.Xaml.Controls.TextBlock { Text = label });
			button.Content = content;

		});

	public static Element UnitNumberBox(double value, Action<double> changed, string unit, int decimals, double smallChange, double minimum, double maximum, string label, bool enabled = true) =>
		Grid([GridSize.Star()], [GridSize.Auto],
			NumberBox(Math.Round(value, decimals, MidpointRounding.AwayFromZero), changed)
				.Range(minimum, maximum).AutomationName(label).Set(box => {
					box.SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline;
					box.SmallChange = smallChange;
					if (box.Tag is not int places || places != decimals) {
						box.NumberFormatter = new DecimalFormatter { FractionDigits = decimals };
						box.Tag = decimals;
					}

				}).OnMountAdd(element => { element.Loaded += HideClearButton; HideClearButton(element, new RoutedEventArgs()); })
				.OnUnmountAdd(element => element.Loaded -= HideClearButton),
			TextBlock(unit).FontSize(12).Foreground(new ThemeRef("TextFillColorTertiaryBrush")).Margin(new Thickness(0, 0, 78, 0)).HorizontalAlignment(HorizontalAlignment.Right).VerticalAlignment(VerticalAlignment.Center).AccessibilityView(AccessibilityView.Raw).Set(text => text.IsHitTestVisible = false)
		).Width(170).IsEnabled(enabled);

	private static void HideClearButton(object sender, RoutedEventArgs args) {
		if (FindByName((DependencyObject)sender, "DeleteButton") is UIElement child && VisualTreeHelper.GetParent(child) is Panel parent) parent.Children.Remove(child);
	}
	private static DependencyObject? FindByName(DependencyObject root, string name) {
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) {
			var child = VisualTreeHelper.GetChild(root, i);
			if (child is FrameworkElement element && element.Name == name) return child;
			if (FindByName(child, name) is { } found) return found;
		}
		return null;
	}
	public static Element LabeledPercentageSlider(string label, double value, Action<double> changed) =>
		Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto, GridSize.Auto],
			BodyStrong(label).HeadingLevel(AutomationHeadingLevel.Level3).Grid(0, 0),
			TextBlock($"{Math.Round(value * 100):0}%").Foreground(new ThemeRef("TextFillColorSecondaryBrush")).Grid(0, 1),
			Slider(value, 0, 1, changed).AutomationName(label).Set(slider => slider.StepFrequency = 0.01).Grid(1, 0, 1, 2)
		).Set(grid => grid.RowSpacing = 6);

	public static Element CompactToggle(bool value, Action<bool> changed, string label) =>
		ToggleSwitch(value, changed).AutomationName(label).MinWidth(0).Width(40).HorizontalAlignment(HorizontalAlignment.Right).Set(toggle => {
			toggle.OnContent = null; toggle.OffContent = null;
		});
}
