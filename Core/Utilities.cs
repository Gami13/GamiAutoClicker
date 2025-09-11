using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Graphics;
using WinRT.Interop;

namespace GamiAutoClicker;

public static class Utilities {

	public static void OpenSettingsWindow(object _, RoutedEventArgs __) {
		CreateWindow(WindowKey.Settings);
	}
	public static void CreateWindow(WindowKey windowKey) {
		bool isOpen = MainWindow.windowThemes.ContainsKey(windowKey);
		if (isOpen) {
			return;
		}
		var config = Constants.WindowConfigs[windowKey];
		var window = config.windowConstructor();
		var themeController = new WindowController(window, windowKey);

		MainWindow.windowThemes[windowKey] = themeController;

		window.Activate();


	}
	public static AppWindow GetAppWindowFromKey(WindowKey key) {
		var window = MainWindow.windowThemes[key].window;
		IntPtr hWnd = WindowNative.GetWindowHandle(window);
		WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
		return AppWindow.GetFromWindowId(windowId);
	}
}
