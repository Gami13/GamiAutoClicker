using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using Windows.Graphics;


namespace Gami;



internal sealed partial class TitleBar : UserControl {
	private readonly object windowKey;
	private bool _isActive = true;

	public TitleBar(object windowKey) {
		InitializeComponent();
		this.Loaded += AppTitleBar_Loaded;
		this.SizeChanged += AppTitleBar_SizeChanged;
		this.Unloaded += AppTitleBar_Unloaded;
		EasyWindows.WindowOptions options = EasyWindows.GetWindowOptions(windowKey);
		this.TitleBarTextBlock.Text = options.Title;
		if (options.Button is { } button) {
			this.TitleBarButton.Visibility = Visibility.Visible;
			this.TitleBarButtonIcon.Symbol = button.Icon;
			this.TitleBarButton.Click += button.Action;
		} else {
			this.TitleBarButton.Visibility = Visibility.Collapsed;
		}
		this.windowKey = windowKey;
	}

	internal void RefreshForeground(bool? isActive = null) {
		if (isActive is { } active) {
			_isActive = active;
		}

		string resourceKey = _isActive
			? "WindowCaptionForeground"
			: "WindowCaptionForegroundDisabled";

		if (Application.Current?.Resources[resourceKey] is SolidColorBrush brush) {
			Foreground = brush;
		}
	}

	private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void SetRegionsForCustomTitleBar() {
		if (AppTitleBar.XamlRoot is not { } xamlRoot) {
			return;
		}

		AppWindow appWindow = EasyWindows.GetAppWindow(windowKey);


		double scaleAdjustment = xamlRoot.RasterizationScale;
		if (scaleAdjustment <= 0) {
			return;
		}


		RightPaddingColumn.Width = new GridLength(appWindow.TitleBar.RightInset / scaleAdjustment);
		LeftPaddingColumn.Width = new GridLength(appWindow.TitleBar.LeftInset / scaleAdjustment);


		GeneralTransform transform = TitleBarButton.TransformToVisual(null);
		Rect bounds = transform.TransformBounds(new Rect(0, 0, TitleBarButton.ActualWidth, TitleBarButton.ActualHeight));
		RectInt32 settingsButtonRect = GetRect(bounds, scaleAdjustment);

		var rectArray = new RectInt32[] { settingsButtonRect };

		var nonClientInputSrc =
			InputNonClientPointerSource.GetForWindowId(appWindow.Id);
		nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);

	}
	private void AppTitleBar_Unloaded(object sender, RoutedEventArgs e) {
		this.Loaded -= AppTitleBar_Loaded;
		this.SizeChanged -= AppTitleBar_SizeChanged;
		this.Unloaded -= AppTitleBar_Unloaded;
		if (EasyWindows.GetWindowOptions(windowKey).Button is { } button) {
			this.TitleBarButton.Click -= button.Action;
		}
	}

	private static RectInt32 GetRect(Rect bounds, double scale) {
		return new Windows.Graphics.RectInt32(
			_X: (int)Math.Round(bounds.X * scale),
			_Y: (int)Math.Round(bounds.Y * scale),
			_Width: (int)Math.Round(bounds.Width * scale),
			_Height: (int)Math.Round(bounds.Height * scale)
		);
	}

}
