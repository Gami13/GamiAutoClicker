using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using Windows.Graphics;
using WinRT.Interop;
using GamiAutoClicker.WindowManager;


namespace GamiAutoClicker.Components;

internal sealed partial class TopWindowBar : UserControl {
	private readonly object windowKey;


	public TopWindowBar(object windowKey) {
		InitializeComponent();
		this.Loaded += AppTitleBar_Loaded;
		this.SizeChanged += AppTitleBar_SizeChanged;
		this.Unloaded += AppTitleBar_Unloaded;
		WindowConfig config = WindowManager.Configuration.WindowConfigs[windowKey];
		this.TitleBarTextBlock.Text = config.title;
		this.TitleBarButton.Visibility = config.hasButton ? Visibility.Visible : Visibility.Collapsed;
		this.TitleBarButtonIcon.Symbol = config.buttonIcon;
		this.TitleBarButton.Click += config.buttonAction;
		this.windowKey = windowKey;
	}
	private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void SetRegionsForCustomTitleBar() {
		AppWindow appWindow = ThemeHelper.GetAppWindow(windowKey);


		if (appWindow == null) { return; }
		double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;


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
		// if (TitleBarButton != null && WindowManager.Configuration.WindowConfigs.TryGetValue(windowKey, out var config)) {
		// 	this.TitleBarButton.Click -= config.buttonAction;
		// }
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
