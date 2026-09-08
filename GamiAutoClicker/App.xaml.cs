using GamiAutoClicker;
using Gami;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GamiAutoClicker {
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	public partial class App : Application {
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>


		public App() {
			InitializeComponent();
			EasyWindows.RegisterWindow(WindowKey.Main, new EasyWindows.WindowOptions {
				Factory = () => new MainWindow(),
				PresenterKind = AppWindowPresenterKind.Default,
				Title = "Gami's AutoClicker",
				Button = new EasyWindows.ButtonOptions {
					Icon = Symbol.Setting,
					Action = Utilities.OpenSettingsWindow
				},
				IsResizable = false,
				IsMinimizable = false,
				IsMaximizable = false,
				DefaultSize = new SizeInt32(370, 330),
				MinimumSize = new SizeInt32(370, 330),
				MaximumSize = new SizeInt32(370, 330),

				DefaultPosition = new PointInt32(100, 100)
			});
			EasyWindows.RegisterWindow(WindowKey.Settings, new EasyWindows.WindowOptions {
				Factory = () => new SettingsWindow(),
				PresenterKind = AppWindowPresenterKind.Overlapped,
				Title = "Settings",
				IsResizable = true,
				IsMinimizable = true,
				IsMaximizable = true,
				DefaultSize = new SizeInt32(860, 600),
				MinimumSize = new SizeInt32(434, 320),
				MaximumSize = new SizeInt32(1110, 1440),

				DefaultPosition = new PointInt32(200, 200)
			});
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
			EasyWindows.CreateWindow(WindowKey.Main);
			
		}
	}
}
