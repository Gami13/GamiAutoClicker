using GamiAutoClicker;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GamiAutoClicker {
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application {
		private Window? _window;

		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>


		public App() {
			InitializeComponent();
			WindowManager.Configuration.WindowConfigs.Add(WindowKey.Main, new WindowManager.WindowConfig {
				windowConstructor = () => new MainWindow(),
				presenterKind = AppWindowPresenterKind.Default,
				title = "Gami's AutoClicker",
				hasButton = true,
				buttonIcon = Symbol.Setting,
				buttonAction = Utilities.OpenSettingsWindow,
				isResizable = false,
				isMinimizable = false,
				isMaximizable = false,
				defaultSize = new SizeInt32(370, 290),
				defaultPosition = new SizeInt32(100, 100)
			});
			WindowManager.Configuration.WindowConfigs.Add(WindowKey.Settings, new WindowManager.WindowConfig {
				windowConstructor = () => new SettingsWindow(),
				presenterKind = AppWindowPresenterKind.Overlapped,
				title = "Settings",
				hasButton = false,
				buttonIcon = Symbol.Delete,
				buttonAction = null,
				isResizable = true,
				isMinimizable = true,
				isMaximizable = true,
				defaultSize = new SizeInt32(400, 300),
				defaultPosition = new SizeInt32(200, 200)
			});
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
			_window = new MainWindow();
			_window.Activate();
		}
	}
}
