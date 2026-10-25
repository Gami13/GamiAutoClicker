using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;
using GamiAutoClicker.Components.Main;
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

		void ChangeCps(double value) {
			if (double.IsFinite(value) && value > 0) ChangeDelay(1000d / value);
		}

		void ChangeOffset(double value) {
			if (!double.IsFinite(value)) return;
			engine.RandomOffsetMilliseconds = Math.Clamp(value, 0, 1_000_000);
			setOffset(engine.RandomOffsetMilliseconds);
		}

		void ChangeMouseButton(int index) {
			if ((uint)index >= MouseButtons.Length) return;
			engine.MouseButton = MouseButtons[index];
			setButtonIndex(index);
		}

		void ChangeHoldMode(bool value) {
			engine.HoldMode = value;
			setHoldMode(value);
		}

		void ChangeEnabled(bool value) {
			engine.Enabled = value;
			setEnabled(value);
			setFailure(null);
		}

		string locale = Localization.Culture.Name;
		string[] mouseLabels = Enumerable.Range(1, MouseButtons.Length)
			.Select(number => Localization.Format("MouseTarget", number)).ToArray();

		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Auto, GridSize.Star(), GridSize.Auto],
			Component<TimingCard, TimingCardProps>(new(
				DelayMilliseconds: delay,
				OffsetMilliseconds: offset,
				ControlsEnabled: !enabled,
				Locale: locale,
				OnDelayChanged: ChangeDelay,
				OnCpsChanged: ChangeCps,
				OnOffsetChanged: ChangeOffset
			)).Grid(0, 0),
			Component<InputCard, InputCardProps>(new(
				MouseLabels: mouseLabels,
				SelectedButtonIndex: buttonIndex,
				HoldMode: holdMode,
				ControlsEnabled: !enabled,
				HoldHint: Localization.Format("HoldHint", GeneralSettings.KeyLabel(engine.HoldKey)),
				Locale: locale,
				OnMouseButtonChanged: ChangeMouseButton,
				OnHoldModeChanged: ChangeHoldMode
			)).Grid(1, 0),
			VStack(0).Grid(2, 0),
			Component<StatusBar, StatusBarProps>(new(
				Enabled: enabled,
				Failure: failure,
				ToggleHint: Localization.Format("ToggleHint", GeneralSettings.KeyLabel(engine.ToggleKey)),
				Locale: locale,
				OnEnabledChanged: ChangeEnabled
			)).Grid(3, 0)
		).Margin(new Thickness(16, 8, 16, 12)).Set(grid => grid.RowSpacing = 8);
	}
}
