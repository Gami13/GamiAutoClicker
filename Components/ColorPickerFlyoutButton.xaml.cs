using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Gami;
using EasyWindows = Gami.EasyWindows;

namespace GamiAutoClicker.Components;

public sealed partial class ColorPickerFlyoutButton : UserControl {

	public static readonly DependencyProperty SelectedColorProperty =
		DependencyProperty.Register(
			nameof(SelectedColor),
			typeof(Color),
			typeof(ColorPickerFlyoutButton),
			new PropertyMetadata(Color.FromArgb(255, 16, 129, 255), OnSelectedColorChanged));

	public Color SelectedColor {
		get => (Color)GetValue(SelectedColorProperty);
		set => SetValue(SelectedColorProperty, value);
	}

	public static readonly DependencyProperty HeaderProperty =
	DependencyProperty.Register(
		nameof(Header),
		typeof(string),
		typeof(ColorPickerFlyoutButton),
		new PropertyMetadata("", OnHeaderChanged));

	public string Header {
		get => (string)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	public event EventHandler<Color>? ColorChanged;

	public ColorPickerFlyoutButton() {
		InitializeComponent();
		Loaded += OnLoaded;
		Unloaded += OnUnloaded;
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
		ColorPickerControl.ColorChanged += OnColorPickerColorChanged;
		ColorPickerFlyout.Opening += OnFlyoutOpening;
		EasyWindows.ThemeChanged += OnEasyWindowsThemeChanged;
		updateColorDisplay();
		HeaderText.Text = Header;
		UpdateFlyoutTheme();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e) {
		ColorPickerControl.ColorChanged -= OnColorPickerColorChanged;
		ColorPickerFlyout.Opening -= OnFlyoutOpening;
		EasyWindows.ThemeChanged -= OnEasyWindowsThemeChanged;
	}

	private void OnFlyoutOpening(object? sender, object e) {
		UpdateFlyoutTheme();
	}

	private void OnEasyWindowsThemeChanged(object? sender, EventArgs e) {
		UpdateFlyoutTheme();
	}

	private void UpdateFlyoutTheme() {
		var elementTheme = EasyWindows.Theme.theme switch {
			Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light => ElementTheme.Light,
			Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark => ElementTheme.Dark,
			_ => ElementTheme.Default
		};

		if (ColorPickerFlyout?.Content is FrameworkElement contentElement) {
			contentElement.RequestedTheme = elementTheme;
		}
	}

	private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ColorPickerFlyoutButton control) {
			control.updateColorDisplay();
		}
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is ColorPickerFlyoutButton control) {
			control.updateHeader();	
		}
	}

	private void OnColorPickerColorChanged(ColorPicker sender, ColorChangedEventArgs args) {
		SelectedColor = args.NewColor;
		ColorChanged?.Invoke(this, args.NewColor);
	}

	private void updateColorDisplay() {
		if (ColorDisplayFill != null) {
			ColorDisplayFill.Fill = new SolidColorBrush(SelectedColor);

		}
		if (ColorPickerControl != null) {

			ColorPickerControl.Color = SelectedColor;
		}
		if (ColorValueText != null) {
			ColorValueText.Text = FormatColor(SelectedColor);
		}
		if (ColorButton != null) {
			AutomationProperties.SetName(ColorButton, $"{Header}: {FormatColor(SelectedColor)}");
		}
	}

	private void updateHeader() {
		if (HeaderText != null) {
			HeaderText.Text = Header;
			if (string.IsNullOrEmpty(Header)) {
				HeaderText.Visibility = Visibility.Collapsed;
			}
			else {
				HeaderText.Visibility = Visibility.Visible;
			}
		}
		updateColorDisplay();
	}

	private static string FormatColor(Color color) => $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";


	//Yoinked from Windows Community Toolkit
	private async void ColorDisplay_Loaded(object sender, RoutedEventArgs e) {


		if (sender is Border border) {
			int width = Convert.ToInt32(border.ActualWidth);
			int height = Convert.ToInt32(border.ActualHeight);

			var bitmap = await Utilities.CreateCheckeredBitmapAsync(
				width,
				height,
				Utilities.CheckerBackgroundColor).ConfigureAwait(true);

			if (bitmap != null) {
				border.Background = await Utilities.BitmapToBrushAsync(bitmap, width, height).ConfigureAwait(true);
			}
		}

		return;
	}
}
