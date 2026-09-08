using GamiAutoClicker;
using Gami;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

using Windows.Graphics;
using GamiToolkit.Settings;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace GamiAutoClicker {
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001", Justification = "Main window closure cancels, awaits and disposes the application clicking lifetime.")]
	public partial class App : Application {
		private readonly ClickEngine _engine = new();
		private readonly System.Threading.CancellationTokenSource _clickingCancellation = new();
		private System.Threading.Tasks.Task? _clickingTask;
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>


		public App() {
#if RELOAD_VERIFICATION
            UnhandledException += (_, args) => System.IO.File.WriteAllText(
                System.IO.Path.Combine(System.AppContext.BaseDirectory, "reload-verification.txt"), "FAIL: " + args.Exception);
#endif
			Localization.Initialize();
			InitializeComponent();
			ThemeSettingsStore.Load();
			EasyWindows.ThemeChanged += (_, _) => ThemeSettingsStore.Save();
			EasyWindows.RegisterWindow(WindowKey.Main, new EasyWindows.WindowOptions {
				ContentFactory = () => new MainPage(_engine),
				Closed = OnMainWindowClosed,
				PresenterKind = AppWindowPresenterKind.Default,
				Title = "Gami's AutoClicker",
				Button = new EasyWindows.ButtonOptions {
					Icon = Symbol.Setting,
					AccessibleNameProvider = () => Localization.Get("Settings"),
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
				ContentFactory = () => new SettingsPage(),
				PresenterKind = AppWindowPresenterKind.Overlapped,
				Title = "Settings",
				TitleProvider = () => Localization.Get("Settings"),
				IsResizable = true,
				IsMinimizable = true,
				IsMaximizable = true,
				DefaultSize = new SizeInt32(860, 600),
				MinimumSize = new SizeInt32(434, 320),
				MaximumSize = new SizeInt32(1110, 1440),

				DefaultPosition = new PointInt32(200, 200)
			});
            // Defer replacement until the language ComboBox's SelectionChanged finishes
            // and GeneralSettings has committed the rest of the settings.
            Localization.Changed += () => Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                .TryEnqueue(EasyWindows.ReloadAllWindows);
		}

		/// <summary>
		/// Invoked when the application is launched.
		/// </summary>
		/// <param name="args">Details about the launch request and process.</param>
		protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
			EasyWindows.CreateWindow(WindowKey.Main);
			_clickingTask = _engine.RunAsync(_clickingCancellation.Token);
#if RELOAD_VERIFICATION
            _ = ReloadVerification.RunAsync(_engine);
#endif
			
		}

		private async void OnMainWindowClosed() {
			_clickingCancellation.Cancel();
			if (_clickingTask is not null) await _clickingTask.ConfigureAwait(true);
			_clickingCancellation.Dispose();
			Exit();
		}
	}
}
