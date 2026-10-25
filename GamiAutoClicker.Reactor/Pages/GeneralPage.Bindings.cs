using GamiAutoClicker.Components.Settings;
using Windows.System;

namespace GamiAutoClicker;

internal sealed partial class GeneralPage {
	private GeneralBindings UseBindings() {
		var (toggle, setToggle) = UseState(GeneralSettings.ToggleKey);
		var (hold, setHold) = UseState(GeneralSettings.HoldKey);
		var (language, setLanguage) = UseState(GeneralSettings.Language);
		var (error, setError) = UseState<string?>(null);
		var (attempt, setAttempt) = UseState(0);
		string[] keyLabels = GeneralSettings.Keys.Select(GeneralSettings.KeyLabel).ToArray();
		string[] languageLabels = Localization.Languages.Select(Localization.LanguageLabel).ToArray();

		void Apply(VirtualKey newToggle, VirtualKey newHold, string newLanguage) {
			if (GeneralSettings.TryApply(newToggle, newHold, newLanguage, out string message))
				setError(null);
			else
				setError(message);
			setToggle(GeneralSettings.ToggleKey);
			setHold(GeneralSettings.HoldKey);
			setLanguage(GeneralSettings.Language);
			// A rejected edit must restore the native selection even when model values did not change.
			setAttempt(attempt + 1);
		}

		void ChangeToggle(int index) {
			if ((uint)index < GeneralSettings.Keys.Length) Apply(GeneralSettings.Keys[index], hold, language);
		}
		void ChangeHold(int index) {
			if ((uint)index < GeneralSettings.Keys.Length) Apply(toggle, GeneralSettings.Keys[index], language);
		}
		void ChangeLanguage(int index) {
			if ((uint)index < Localization.Languages.Length) Apply(toggle, hold, Localization.Languages[index]);
		}
		void RestoreDefaults() => Apply(VirtualKey.F8, VirtualKey.XButton1, "system");

		string locale = Localization.Culture.Name;
		var keys = new KeyBindingsCardProps(
			Choices: keyLabels,
			ToggleIndex: Array.IndexOf(GeneralSettings.Keys, toggle),
			HoldIndex: Array.IndexOf(GeneralSettings.Keys, hold),
			Locale: locale,
			EditVersion: attempt,
			OnToggleChanged: ChangeToggle,
			OnHoldChanged: ChangeHold);
		var languages = new LanguageCardProps(
			Choices: languageLabels,
			SelectedIndex: Array.IndexOf(Localization.Languages, language),
			Locale: locale,
			EditVersion: attempt,
			OnChanged: ChangeLanguage);

		return new GeneralBindings(keys, languages, error, attempt, RestoreDefaults);
	}

	private sealed record GeneralBindings(
		KeyBindingsCardProps Keys,
		LanguageCardProps Language,
		string? Error,
		int Attempt,
		Action RestoreDefaults);
}
