using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using Windows.UI;
using Gami;
using EasyWindows = Gami.EasyWindows;

namespace GamiAutoClicker.Components;

internal sealed partial class ColorPickerFlyoutButton : UserControl
{
	private readonly SolidColorBrush _colorDisplayBrush = new();
	private ElementTheme? _flyoutTheme;

	public Color Color
	{
		get => ColorPickerControl.Color;
		set => ColorPickerControl.Color = value;
	}

	public static readonly DependencyProperty HeaderProperty =
	DependencyProperty.Register(
		nameof(Header),
		typeof(string),
		typeof(ColorPickerFlyoutButton),
		new PropertyMetadata("", OnHeaderChanged));

	public string Header
	{
		get => (string)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	public event TypedEventHandler<ColorPicker, ColorChangedEventArgs>? ColorChanged
	{
		add => ColorPickerControl.ColorChanged += value;
		remove => ColorPickerControl.ColorChanged -= value;
	}

	public ColorPickerFlyoutButton()
	{
		InitializeComponent();
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		ColorPickerControl.ColorChanged += OnColorPickerColorChanged;
		ColorPickerFlyout.Opening += OnFlyoutOpening;
		EasyWindows.ThemeChanged += OnEasyWindowsThemeChanged;
		UpdateColorDisplay(ColorPickerControl.Color);
		UpdateHeader();
		UpdateFlyoutTheme();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		ColorPickerControl.ColorChanged -= OnColorPickerColorChanged;
		ColorPickerFlyout.Opening -= OnFlyoutOpening;
		EasyWindows.ThemeChanged -= OnEasyWindowsThemeChanged;
	}

	private void OnFlyoutOpening(object? sender, object e)
	{
		UpdateFlyoutTheme();
	}

	private void OnEasyWindowsThemeChanged(object? sender, EventArgs e)
	{
		UpdateFlyoutTheme();
	}

	private void UpdateFlyoutTheme()
	{
		var elementTheme = EasyWindows.Theme.Theme switch
		{
			Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light => ElementTheme.Light,
			Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark => ElementTheme.Dark,
			_ => ElementTheme.Default
		};

		if (_flyoutTheme == elementTheme) return;

		if (ColorPickerFlyout?.Content is FrameworkElement contentElement)
		{
			contentElement.RequestedTheme = elementTheme;
			_flyoutTheme = elementTheme;
		}
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is ColorPickerFlyoutButton control)
		{
			control.UpdateHeader();
		}
	}

	private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args)
	{
		UpdateColorDisplay(args.NewColor);
	}

	private void UpdateColorDisplay(Color color)
	{
		var formattedColor = FormatColor(color);

		if (ColorDisplayFill != null)
		{
			_colorDisplayBrush.Color = color;
			ColorDisplayFill.Fill = _colorDisplayBrush;
		}
		if (ColorValueText != null)
		{
			ColorValueText.Text = formattedColor;
		}
		if (ColorButton != null)
		{
			AutomationProperties.SetName(ColorButton, $"{Header}: {formattedColor}");
		}
	}

	private void UpdateHeader()
	{
		if (HeaderText != null)
		{
			HeaderText.Text = Header;
			if (string.IsNullOrEmpty(Header))
			{
				HeaderText.Visibility = Visibility.Collapsed;
			}
			else
			{
				HeaderText.Visibility = Visibility.Visible;
			}
		}
	}

	private static string FormatColor(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";


	//Yoinked from Windows Community Toolkit
	private async void ColorDisplay_Loaded(object sender, RoutedEventArgs e)
	{


		if (sender is Border border)
		{
			int width = Convert.ToInt32(border.ActualWidth);
			int height = Convert.ToInt32(border.ActualHeight);

			var bitmap = await Utilities.CreateCheckeredBitmapAsync(
				width,
				height,
				Utilities.CheckerBackgroundColor).ConfigureAwait(true);

			if (bitmap != null)
			{
				border.Background = await Utilities.BitmapToBrushAsync(bitmap, width, height).ConfigureAwait(true);
			}
		}

		return;
	}
}
