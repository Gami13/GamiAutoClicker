using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;
using Windows.UI;

namespace GamiAutoClicker;

public sealed partial class MainWindow : Window {

	private void applyWindowStyle(MainWindow context) {
		IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(context);
		AppWindow appWindow = AppWindow.GetFromWindowId(Win32Interop.GetWindowIdFromWindow(hWnd));

		// Set a fixed size
		SizeInt32 windowSize = new SizeInt32 { Width = 370, Height = 290 };
		appWindow.Resize(windowSize);

		// Optional: prevent resizing
		var presenter = appWindow.Presenter as OverlappedPresenter;
		if (presenter != null) {
			presenter.IsResizable = false;
		}
		appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
		appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
		appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

		SystemBackdrop = new DesktopAcrylicBackdrop();
	}


	public MainWindow() {
		InitializeComponent();
		applyWindowStyle(this);
	}
}

