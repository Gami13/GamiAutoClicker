using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using Windows.Graphics;


namespace GamiAutoClicker.Components;

public sealed partial class TopWindowBar : UserControl {


	public TopWindowBar(WindowKey windowKey) {
		InitializeComponent();
		this.Loaded += AppTitleBar_Loaded;
		this.SizeChanged += AppTitleBar_SizeChanged;
		this.Unloaded += AppTitleBar_Unloaded;
		var config = Constants.WindowConfigs[windowKey];
		this.TitleBarTextBlock.Text = config.title;
		this.TitleBarButton.Visibility = config.hasButton ? Visibility.Visible : Visibility.Collapsed;
		this.TitleBarButtonIcon.Symbol = config.buttonIcon;
		this.TitleBarButton.Click += config.buttonAction;
	}
	private void AppTitleBar_Loaded(object sender, RoutedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void AppTitleBar_SizeChanged(object sender, SizeChangedEventArgs e) {
		SetRegionsForCustomTitleBar();
	}

	private void SetRegionsForCustomTitleBar() {
		// Specify the interactive regions of the title bar.
		if (MainWindow.appWindow == null) return;
		double scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;


		RightPaddingColumn.Width = new GridLength(MainWindow.appWindow.TitleBar.RightInset / scaleAdjustment);
		LeftPaddingColumn.Width = new GridLength(MainWindow.appWindow.TitleBar.LeftInset / scaleAdjustment);


		GeneralTransform transform = TitleBarButton.TransformToVisual(null);
		Rect bounds = transform.TransformBounds(new Rect(0, 0, TitleBarButton.ActualWidth, TitleBarButton.ActualHeight));
		RectInt32 settingsButtonRect = GetRect(bounds, scaleAdjustment);



		var rectArray = new RectInt32[] { settingsButtonRect };

		InputNonClientPointerSource nonClientInputSrc =
			InputNonClientPointerSource.GetForWindowId(MainWindow.appWindow.Id);
		nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, rectArray);

	}
	private void AppTitleBar_Unloaded(object sender, RoutedEventArgs e) {
		this.Loaded -= AppTitleBar_Loaded;
		this.SizeChanged -= AppTitleBar_SizeChanged;
		this.Unloaded -= AppTitleBar_Unloaded;
		// if (TitleBarButton != null && Constants.WindowConfigs.TryGetValue(windowKey, out var config)) {
		// 	this.TitleBarButton.Click -= config.buttonAction;
		// }
	}


	private RectInt32 GetRect(Rect bounds, double scale) {
		return new Windows.Graphics.RectInt32(
			_X: (int)Math.Round(bounds.X * scale),
			_Y: (int)Math.Round(bounds.Y * scale),
			_Width: (int)Math.Round(bounds.Width * scale),
			_Height: (int)Math.Round(bounds.Height * scale)
		);
	}

}
