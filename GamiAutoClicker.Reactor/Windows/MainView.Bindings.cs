using Windows.System;
using GamiAutoClicker.Components.Main;

namespace GamiAutoClicker;

internal sealed partial class MainView {
	private static readonly VirtualKey[] MouseButtons =
	[
		VirtualKey.LeftButton, VirtualKey.RightButton, VirtualKey.MiddleButton,
		VirtualKey.XButton1, VirtualKey.XButton2
	];

	private MainBindings UseBindings() {
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

		var timing = new TimingCardProps(
			DelayMilliseconds: delay,
			OffsetMilliseconds: offset,
			ControlsEnabled: !enabled,
			Locale: locale,
			OnDelayChanged: ChangeDelay,
			OnCpsChanged: ChangeCps,
			OnOffsetChanged: ChangeOffset);
		var input = new InputCardProps(
			MouseLabels: mouseLabels,
			SelectedButtonIndex: buttonIndex,
			HoldMode: holdMode,
			ControlsEnabled: !enabled,
			HoldHint: Localization.Format("HoldHint", GeneralSettings.KeyLabel(engine.HoldKey)),
			Locale: locale,
			OnMouseButtonChanged: ChangeMouseButton,
			OnHoldModeChanged: ChangeHoldMode);
		var status = new StatusBarProps(
			Enabled: enabled,
			Failure: failure,
			ToggleHint: Localization.Format("ToggleHint", GeneralSettings.KeyLabel(engine.ToggleKey)),
			Locale: locale,
			OnEnabledChanged: ChangeEnabled);

		return new MainBindings(timing, input, status);
	}

	private sealed record MainBindings(TimingCardProps Timing, InputCardProps Input, StatusBarProps Status);
}
