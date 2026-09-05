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

	public TitleBar(object windowKey) {
		InitializeComponent();
		this.windowKey = windowKey;
		EasyWindows.WindowOptions options = EasyWindows.GetWindowOptions(windowKey);
		this.TitleBarTextBlock.Text = options.Title;

		if (options.Button is { } button) {
			this.TitleBarButton.Visibility = Visibility.Visible;
			this.TitleBarButtonIcon.Symbol = button.Icon;
			this.TitleBarButton.Click += button.Action;
		}
	}

	internal void SetActive(bool isActive) {
		VisualStateManager.GoToState(this, isActive ? "Active" : "Inactive", false);
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Bound via XAML")]
	private void AppTitleBar_Loaded(object _, RoutedEventArgs __) {
		SetRegionsForCustomTitleBar();
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Bound via XAML")]

	private void AppTitleBar_Unloaded(object _, RoutedEventArgs __) {
		if (EasyWindows.GetWindowOptions(windowKey).Button is { } button) {
			this.TitleBarButton.Click -= button.Action;
		}
	}
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Bound via XAML")]

	private void AppTitleBar_SizeChanged(object _, SizeChangedEventArgs __) {
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

	private static RectInt32 GetRect(Rect bounds, double scale) {
		return new Windows.Graphics.RectInt32(
			_X: (int)Math.Round(bounds.X * scale),
			_Y: (int)Math.Round(bounds.Y * scale),
			_Width: (int)Math.Round(bounds.Width * scale),
			_Height: (int)Math.Round(bounds.Height * scale)
		);
	}

}
