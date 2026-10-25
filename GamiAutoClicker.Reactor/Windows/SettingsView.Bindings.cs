using Microsoft.UI.Xaml;
using NativeNavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using GamiToolkit.Components;

namespace GamiAutoClicker;

internal sealed partial class SettingsView {
	private bool _navigationLoaded;

	private SettingsBindings UseBindings() {
		SettingsWindowState retained = AppState.SettingsWindow;
		var (section, setSection) = UseState(retained.SelectedSection);
		var (paneOpen, setPaneOpen) = UseState(retained.IsPaneOpen);
		var (saveError, setSaveError) = UseState<string?>(null);
		var (saveAttempt, setSaveAttempt) = UseState(0);
		var (_, setLanguageVersion) = UseState(0);

		UseEffect(() => {
			int languageVersion = 0;
			if (section == SettingsSection.General) GeneralSettings.BeginEditing(this);
			void LanguageChanged() {
				setLanguageVersion(++languageVersion);
			}
			Localization.Changed += LanguageChanged;
			if (Environment.GetEnvironmentVariable("GAMI_PERF_SCENARIO") == "settings" &&
				Environment.GetEnvironmentVariable("GAMI_PERF_READY_FILE") is { Length: > 0 } readyFile)
				File.WriteAllText(readyFile, "settings");
			return () => {
				Localization.Changed -= LanguageChanged;
				GeneralSettings.EndEditing(this);
			};
		}, []);

		void ChangeSection(SettingsSection selected) {
			if (selected == SettingsSection.General) GeneralSettings.BeginEditing(this);
			else GeneralSettings.EndEditing(this);
			retained.SelectedSection = selected;
			setSection(selected);
		}

		void Save() {
			setSaveError(SettingsStore.TrySave(out string error) ? null : error);
			setSaveAttempt(saveAttempt + 1);
		}

		void ChangeSelectedTag(string? tag) =>
			ChangeSection(tag == "general" ? SettingsSection.General : SettingsSection.Appearance);

		void ChangePaneOpen(bool open) {
			if (!_navigationLoaded) return;
			retained.IsPaneOpen = open;
			setPaneOpen(open);
		}

		void NavigationLoaded() {
			_navigationLoaded = true;
			setPaneOpen(retained.IsPaneOpen);
		}

		void ConfigureNavigation(NativeNavigationView view) {
			if (view.Tag is not NavigationViewBehavior behavior) {
				behavior = new NavigationViewBehavior(view, retained.IsPaneOpen, NavigationLoaded);
				view.Tag = behavior;
			}
			behavior.Update(
				Localization.Get("SettingsSaveButton.Content"),
				Localization.Get("SettingsSaveHint.Text"),
				Save);
		}

		void ReleaseNavigation(FrameworkElement element) {
			if (element is NativeNavigationView view && view.Tag is NavigationViewBehavior behavior) {
				behavior.Dispose();
				view.Tag = null;
				_navigationLoaded = false;
			}
		}

		return new SettingsBindings(
			Section: section,
			PaneOpen: paneOpen,
			SaveError: saveError,
			SaveAttempt: saveAttempt,
			SelectSection: ChangeSelectedTag,
			ChangePaneOpen: ChangePaneOpen,
			ConfigureNavigation: ConfigureNavigation,
			ReleaseNavigation: ReleaseNavigation);
	}

	private sealed record SettingsBindings(
		SettingsSection Section,
		bool PaneOpen,
		string? SaveError,
		int SaveAttempt,
		Action<string?> SelectSection,
		Action<bool> ChangePaneOpen,
		Action<NativeNavigationView> ConfigureNavigation,
		Action<FrameworkElement> ReleaseNavigation);
}
