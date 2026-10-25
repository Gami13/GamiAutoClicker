using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using NativeNavigationView = Microsoft.UI.Xaml.Controls.NavigationView;
using static Microsoft.UI.Reactor.Factories;
using GamiToolkit.Components;

namespace GamiAutoClicker;

internal sealed class SettingsView : Component {
	private bool _navigationLoaded;

	public override Element Render() {
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

		return (NavigationView([
					NavItem(Localization.Get("SettingsWindow_GeneralNavigationItem.Content"), "Setting", "general"),
					NavItem(Localization.Get("SettingsWindow_AppearanceNavigationItem.Content"), "Edit", "appearance")
				],
				Grid([GridSize.Star()], [GridSize.Auto, GridSize.Star()],
					saveError is not null ? (InfoBar(null, saveError) with { Key = "save-" + saveAttempt, Severity = Microsoft.UI.Xaml.Controls.InfoBarSeverity.Error }).Grid(0, 0) : null,
					(section == SettingsSection.General ? Component<GeneralPage>() : Component<AppearancePage>()).Grid(1, 0)
				)) with {
			SelectedTag = section == SettingsSection.General ? "general" : "appearance",
			OnSelectedTagChanged = ChangeSelectedTag,
			IsPaneOpen = paneOpen,
			OnPaneOpenChanged = ChangePaneOpen,
			IsSettingsVisible = false,
			IsBackButtonVisible = Microsoft.UI.Xaml.Controls.NavigationViewBackButtonVisible.Collapsed,
			IsTitleBarAutoPaddingEnabled = false,
			OpenPaneLength = 150,
			CompactModeThresholdWidth = 0,
			ExpandedModeThresholdWidth = 750,

		})
				.PaneTitle(Localization.Get("Settings"))
				.Set(ConfigureNavigation)
				.OnUnmountAdd(ReleaseNavigation);
	}
}
