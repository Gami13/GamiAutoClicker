using Gami;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;
using static Microsoft.UI.Reactor.Factories;
namespace GamiAutoClicker;

internal static class AppState {
	internal static ClickEngine Engine { get; } = new();
	internal static SettingsWindowState SettingsWindow { get; } = new();
	private static readonly CancellationTokenSource EngineCancellation = new();
	private static Task? _engineTask;
	internal static void StartEngine() => _engineTask ??= Engine.RunAsync(EngineCancellation.Token);
	private static async void StopEngine() {
		EngineCancellation.Cancel();
		if (_engineTask is not null) await _engineTask;
		EngineCancellation.Dispose();
	}
	internal static void Initialize() {
		SettingsStore.Load();
		Engine.IdlePollIntervalMilliseconds = 25;
		Localization.Changed += EasyWindows.RefreshTitles;
		Engine.ToggleKey = GeneralSettings.ToggleKey;
		Engine.HoldKey = GeneralSettings.HoldKey;
		EasyWindows.RegisterWindow(WindowKey.Main, new EasyWindows.WindowOptions {
			ContentFactory = () => Component<MainView>(),
			Title = "Gami's AutoClicker",
			DefaultSize = new SizeInt32(370, 370),
			MinimumSize = new SizeInt32(370, 370),
			MaximumSize = new SizeInt32(370, 370),
			Closed = StopEngine,
			DefaultPosition = new PointInt32(100, 100),
			IsResizable = false,
			IsMinimizable = false,
			IsMaximizable = false,
			Button = new EasyWindows.ButtonOptions { Icon = Symbol.Setting, AccessibleNameProvider = () => Localization.Get("Settings"), Action = OpenSettings }
		});
		EasyWindows.RegisterWindow(WindowKey.Settings, new EasyWindows.WindowOptions<SettingsWindowState> {
			State = SettingsWindow,
			ContentFactory = _ => Component<SettingsView>(),
			Title = "Settings",
			TitleProvider = () => Localization.Get("Settings"),
			DefaultSize = new SizeInt32(860, 600),
			MinimumSize = new SizeInt32(434, 320),
			MaximumSize = new SizeInt32(1110, 1440),
			DefaultPosition = new PointInt32(200, 200),
			Closed = SettingsStore.DiscardChanges
		});
	}
	internal static void OpenSettings() => EasyWindows.CreateWindow(WindowKey.Settings);
}
internal enum WindowKey { Main, Settings }
internal sealed class MainWindow : RegisteredWindow { public MainWindow() : base(WindowKey.Main) { } }
internal sealed class SettingsWindowState {
	internal SettingsSection SelectedSection { get; set; } = SettingsSection.Appearance;
	internal bool IsPaneOpen { get; set; } = true;
}
internal enum SettingsSection { Appearance, General }
