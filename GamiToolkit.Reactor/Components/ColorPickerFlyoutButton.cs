using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;
namespace GamiToolkit.Components;

public static class ColorPickerFlyoutButton {
	public static Element Create(string label, Color color, Action<Color> changed) => VStack(6,
		TextBlock(label).HeadingLevel(AutomationHeadingLevel.Level3),
		Button("").Set(button => {
			if (button.Tag is not PickerState state) { state = new PickerState(button); button.Tag = state; }
			state.Update(label, color, changed);
		}));

	private sealed class PickerState {
		private readonly Microsoft.UI.Xaml.Controls.Button _button;
		private readonly Microsoft.UI.Xaml.Controls.ColorPicker _picker;
		private readonly Microsoft.UI.Xaml.Controls.TextBlock _value = new() { VerticalAlignment = VerticalAlignment.Center, FontSize = 12, TextTrimming = TextTrimming.CharacterEllipsis };
		private readonly SolidColorBrush _swatch = new();
		private readonly Microsoft.UI.Xaml.Controls.Flyout _flyout;
		private readonly Microsoft.UI.Xaml.Controls.Grid _content;
		private Action<Color>? _changed;
		private bool _updating;
		internal PickerState(Microsoft.UI.Xaml.Controls.Button button) {
			_button = button;
			_picker = new Microsoft.UI.Xaml.Controls.ColorPicker { ColorSpectrumShape = ColorSpectrumShape.Ring, IsAlphaEnabled = true, IsColorChannelTextInputVisible = true, IsColorSliderVisible = true, IsHexInputVisible = true, IsMoreButtonVisible = true };
			var flyout = new Microsoft.UI.Xaml.Controls.Flyout { Content = _picker, Placement = FlyoutPlacementMode.Bottom, ShouldConstrainToRootBounds = false, SystemBackdrop = new EasyWindowsBackdrop() };
			flyout.FlyoutPresenterStyle = ResourceStyles.ColorFlyoutPresenterStyle();
			_flyout = flyout;
			button.Flyout = flyout; button.Padding = new Thickness(6); button.HorizontalAlignment = HorizontalAlignment.Stretch; button.HorizontalContentAlignment = HorizontalAlignment.Stretch;
			var content = new Microsoft.UI.Xaml.Controls.Grid { ColumnSpacing = 8 };
			content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
			content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
			var swatch = new Microsoft.UI.Xaml.Controls.Grid { Width = 30, Height = 26 };
			for (int row = 0; row < 7; row++) for (int col = 0; col < 8; col++) {
				var tile = new Microsoft.UI.Xaml.Shapes.Rectangle { Width = 4, Height = 4, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Margin = new Thickness(col * 4, row * 4, 0, 0), Fill = new SolidColorBrush((row + col) % 2 == 0 ? Microsoft.UI.Colors.Transparent : Color.FromArgb(0x19, 0x80, 0x80, 0x80)) };
				swatch.Children.Add(tile);
			}
			swatch.Children.Add(new Microsoft.UI.Xaml.Shapes.Rectangle { Fill = _swatch });
			content.Children.Add(new Microsoft.UI.Xaml.Controls.Border { Width = 32, Height = 28, Child = swatch, BorderBrush = ThemeResource.Brush("ButtonBorderBrush"), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(6) });
			Microsoft.UI.Xaml.Controls.Grid.SetColumn(_value, 1); content.Children.Add(_value);
			var chevron = new Microsoft.UI.Xaml.Controls.FontIcon { Glyph = "\uE70D", FontSize = 12, VerticalAlignment = VerticalAlignment.Center };
			Microsoft.UI.Xaml.Controls.Grid.SetColumn(chevron, 2); content.Children.Add(chevron);
			_content = content; button.Content = content;
			_picker.ColorChanged += (_, args) => { if (!_updating) _changed?.Invoke(args.NewColor); };
			button.Loaded += (_, _) => { EasyWindows.ThemeChanged += ThemeChanged; RefreshTheme(); };
			button.Unloaded += (_, _) => EasyWindows.ThemeChanged -= ThemeChanged;
			flyout.Opening += (_, _) => RefreshTheme();
		}
		internal void Update(string label, Color color, Action<Color> changed) {
			_changed = changed;
			if (!ReferenceEquals(_button.Content, _content)) _button.Content = _content;
			if (!ReferenceEquals(_button.Flyout, _flyout)) _button.Flyout = _flyout;
			_updating = true; try { if (_picker.Color != color) _picker.Color = color; } finally { _updating = false; }
			_swatch.Color = color; _value.Text = $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
			AutomationProperties.SetName(_button, $"{label}: {_value.Text}");
		}
		private void ThemeChanged(object? sender, EventArgs args) => RefreshTheme();
		private void RefreshTheme() {
			_picker.RequestedTheme = EasyWindows.Theme.Theme switch { SystemBackdropTheme.Dark => ElementTheme.Dark, SystemBackdropTheme.Light => ElementTheme.Light, _ => ElementTheme.Default };
			_value.Foreground = ThemeResource.Brush("TextFillColorSecondaryBrush");
		}
	}
}
