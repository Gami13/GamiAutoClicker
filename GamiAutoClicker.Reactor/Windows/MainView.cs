using Microsoft.UI;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml.Media;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;
using static GamiToolkit.Components.SettingsControls;
using Microsoft.UI.Xaml;

namespace GamiAutoClicker;

internal sealed class MainView : Component {
	private static readonly VirtualKey[] MouseButtons =
	[
		VirtualKey.LeftButton, VirtualKey.RightButton, VirtualKey.MiddleButton,
		VirtualKey.XButton1, VirtualKey.XButton2
	];

	public override Element Render() {
		ClickEngine engine = AppState.Engine;
		ReactorWindow window = UseWindow()!;
		var (delay, setDelay) = UseState(engine.IntervalMilliseconds);
		var (offset, setOffset) = UseState(engine.RandomOffsetMilliseconds);
		var (buttonIndex, setButtonIndex) = UseState(Math.Max(0, Array.IndexOf(MouseButtons, engine.MouseButton)));
		var (holdMode, setHoldMode) = UseState(engine.HoldMode);
		var (enabled, setEnabled) = UseState(engine.Enabled);
		var (failure, setFailure) = UseState<string?>(null);
		var (_, setLanguageVersion) = UseState(0);
		var (_, setGeneralVersion) = UseState(0);

		UseEffect(() => {

			void ToggleRequested() {
				engine.Enabled = !engine.Enabled;
				setEnabled(engine.Enabled);
				setFailure(null);
			}
			void ClickingFailed(string message) {
				setEnabled(false);
				setFailure(message);
			}
			int generalVersion = 0;
			void GeneralChanged() {
				engine.Enabled = false;
				engine.ToggleKey = GeneralSettings.ToggleKey;
				engine.HoldKey = GeneralSettings.HoldKey;
				setEnabled(false);
				setGeneralVersion(++generalVersion);
			}
			int languageVersion = 0;
			void LanguageChanged() => setLanguageVersion(++languageVersion);

			engine.ToggleRequested += ToggleRequested;
			engine.ClickingFailed += ClickingFailed;
			GeneralSettings.Changed += GeneralChanged;
			Localization.Changed += LanguageChanged;
			AppState.StartEngine();
			if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "settings")
				AppState.OpenSettings();
			if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "main" &&
				Environment.GetEnvironmentVariable("GAMI_PERF_READY_FILE") is { Length: > 0 } readyFile)
				File.WriteAllText(readyFile, "main");
			return () => {
				engine.ToggleRequested -= ToggleRequested;
				engine.ClickingFailed -= ClickingFailed;
				GeneralSettings.Changed -= GeneralChanged;
				Localization.Changed -= LanguageChanged;


			};
		}, []);

		void ChangeDelay(double value) {
			if (!double.IsFinite(value) || value < ClickEngine.MinimumIntervalMilliseconds) return;
			engine.IntervalMilliseconds = value;
			setDelay(value);
		}

		string[] mouseLabels = Enumerable.Range(1, MouseButtons.Length)
			.Select(number => Localization.Format("MouseTarget", number)).ToArray();
		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Auto, GridSize.Star(), GridSize.Auto],
			SettingsCard(
				Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto, GridSize.Auto, GridSize.Auto],
					TextBlock(Localization.Get("MainWindow_Localized1.Text")).VerticalAlignment(VerticalAlignment.Center).Grid(0, 0),
					UnitNumberBox(delay, ChangeDelay, "ms", 0, 1, ClickEngine.MinimumIntervalMilliseconds, 1_000_000, Localization.Get("MainWindow_Localized1.Text"), !enabled).Grid(0, 1),
					TextBlock(Localization.Get("MainWindow_Localized2.Text")).VerticalAlignment(VerticalAlignment.Center).Grid(1, 0),
					UnitNumberBox(1000d / delay, value => { if (double.IsFinite(value) && value > 0) ChangeDelay(1000d / value); }, "CPS", 3, 0.001, 0.001, ClickEngine.MaximumClicksPerSecond, Localization.Get("MainWindow_Localized2.Text"), !enabled).Grid(1, 1),
					TextBlock(Localization.Get("MainWindow_RandomOffset.Text")).VerticalAlignment(VerticalAlignment.Center).Grid(2, 0),
					UnitNumberBox(offset, value => { if (!double.IsFinite(value)) return; engine.RandomOffsetMilliseconds = Math.Clamp(value, 0, 1_000_000); setOffset(engine.RandomOffsetMilliseconds); }, "ms", 0, 1, 0, 1_000_000, Localization.Get("RandomOffset"), !enabled).ToolTip(Localization.Get("RandomOffsetHint")).Grid(2, 1)
				).Set(g => g.RowSpacing = 8)
			).Padding(new Thickness(12, 8, 12, 8)).Grid(0, 0),
			SettingsCard(
				Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto, GridSize.Auto],
					TextBlock(Localization.Get("MainWindow_Localized3.Text")).VerticalAlignment(VerticalAlignment.Center).Grid(0, 0),
					ComboBox(mouseLabels, buttonIndex, index => { if ((uint)index >= MouseButtons.Length) return; engine.MouseButton = MouseButtons[index]; setButtonIndex(index); }).Width(120).IsEnabled(!enabled).AutomationName(Localization.Get("Clicktarget")).Grid(0, 1),
					HStack(5, TextBlock(Localization.Get("MainWindow_Localized4.Text")).VerticalAlignment(VerticalAlignment.Center),
						TextBlock("\uE946").FontFamily("Segoe Fluent Icons").FontSize(12).Foreground(new ThemeRef("TextFillColorSecondaryBrush")).VerticalAlignment(VerticalAlignment.Center))
						.ToolTip(Localization.Format("HoldHint", GeneralSettings.KeyLabel(engine.HoldKey))).Grid(1, 0),
					CompactToggle(holdMode, value => { engine.HoldMode = value; setHoldMode(value); }, Localization.Get("Holdmode")).IsEnabled(!enabled).Grid(1, 1)
				).Set(g => g.RowSpacing = 8)
			).Padding(new Thickness(12, 8, 12, 8)).Grid(1, 0),
			VStack(0).Grid(2, 0),
			SettingsCard(
				Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
					HStack(8, TextBlock(Localization.Get("MainWindow_Localized5.Text")).VerticalAlignment(VerticalAlignment.Center),
						CompactToggle(enabled, value => { engine.Enabled = value; setEnabled(value); setFailure(null); }, Localization.Get("Enableclicking"))
						.ToolTip(Localization.Format("ToggleHint", GeneralSettings.KeyLabel(engine.ToggleKey)))).Grid(0, 0),
					HStack(6, Ellipse().Width(8).Height(8).Fill(ThemeResource.Brush(enabled ? "SystemFillColorSuccessBrush" : "SystemFillColorCriticalBrush")).VerticalAlignment(VerticalAlignment.Center),
						TextBlock(failure is null ? Localization.Get(enabled ? "Enabled" : "Disabled") : Localization.Get("Blocked")).VerticalAlignment(VerticalAlignment.Center).ToolTip(failure ?? string.Empty)).Grid(0, 1)
				)
			).Background(new ThemeRef("LayerFillColorDefaultBrush")).Padding(new Thickness(12, 6, 12, 6)).Grid(3, 0)
		).Margin(new Thickness(16, 8, 16, 12)).Set(g => g.RowSpacing = 8);
	}
}
