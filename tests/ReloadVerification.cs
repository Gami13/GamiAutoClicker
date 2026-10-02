using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Gami;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker;

// Runs inside the real WinUI dispatcher via the opt-in ReloadVerification build.
internal static class ReloadVerification {
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031", Justification = "Report any integration failure after restoring the language preference.")]
	internal static async Task RunAsync(ClickEngine engine) {
		string preference = Localization.Preference;
		string result = "FAIL: verification did not complete.";
		try {
			Require(engine.ToggleKey == GeneralSettings.ToggleKey && engine.HoldKey == GeneralSettings.HoldKey,
				"Startup must apply saved hotkeys before running the engine.");
			await Settle().ConfigureAwait(true);
			await VerifySettingsPersistence().ConfigureAwait(true);
			VerifyMainPageControls();
			VerifyRandomOffset();
			EasyWindows.WindowOptionsBase options = EasyWindows.Windows[WindowKey.Settings];
			SettingsWindowState state = options.GetState<SettingsWindowState>();
			Require(ReferenceEquals(state, options.GetState<SettingsWindowState>()), "Lookup must return the same object.");
			Require(ReferenceEquals(state, options.GetState<object>()), "Compatible base types must refer to the same state.");
			Expect<InvalidOperationException>(() => options.GetState<string>());
			Expect<InvalidOperationException>(() => EasyWindows.Windows[WindowKey.Main].GetState<SettingsWindowState>());
			Expect<KeyNotFoundException>(() => _ = EasyWindows.Windows[new object()]);
			Expect<NotSupportedException>(() => ((IDictionary<object, EasyWindows.WindowOptionsBase>)EasyWindows.Windows).Clear());
			state.SelectedSection = SettingsSection.General;
			Require(options.GetState<SettingsWindowState>().SelectedSection == SettingsSection.General, "Mutating a local reference must update registered state.");
			state.SelectedSection = SettingsSection.Appearance;
			Window main = GetWindow(WindowKey.Main);
			MainPage mainContent = Content<MainPage>(main);
			double interval = engine.IntervalMilliseconds;
			EasyWindows.CreateWindow(WindowKey.Settings);
			await Settle().ConfigureAwait(true);
			Window settings = GetWindow(WindowKey.Settings);
			UIElement shell = settings.Content;
			var id = settings.AppWindow.Id;
			var bounds = settings.AppWindow.Size;
			var position = settings.AppWindow.Position;
			SettingsPage original = Content<SettingsPage>(settings);
			Require(!IsGeneralSelected(original) && state.SelectedSection == SettingsSection.Appearance, "Initial section must be Appearance.");
			Require(IsPaneOpen(original) && state.IsPaneOpen, "Pane must initially open from model defaults.");
			state.IsPaneOpen = false;
			Require(!IsPaneOpen(original), "Observable pane state must update the live control.");
			state.IsPaneOpen = true;
			state.SelectedSection = SettingsSection.General;
			await Settle().ConfigureAwait(true);
			Require(IsGeneralSelected(original) && SelectedPage(original) is Pages.Settings.GeneralPage && GeneralSettings.IsEditing,
				"Observable selection must navigate and pause input without a reload.");
			state.SelectedSection = SettingsSection.Appearance;
			await Settle().ConfigureAwait(true);
			Require(!IsGeneralSelected(original) && !GeneralSettings.IsEditing, "Observable navigation away from General must release input.");
			await VerifyIndependentWindow(state).ConfigureAwait(true);
			((NavigationView)original.FindName("SettingsNavigation")).IsPaneOpen = false;
			Require(!state.IsPaneOpen, "Closing the pane must update the model.");
			Select(original, "GeneralNavigationItem");
			await Settle().ConfigureAwait(true);
			Require(state.SelectedSection == SettingsSection.General && GeneralSettings.IsEditing, "Navigation must update registered state and pause input.");
			Page general = SelectedPage(original);
			string oldLabel = ((TextBlock)general.FindName("Localized1")).Text;
			string next = Localization.Culture.TwoLetterISOLanguageName == "pl" ? "en" : "pl";
			((ComboBox)general.FindName("LanguageComboBox")).SelectedIndex = Array.IndexOf(Localization.Languages, next);
			await Settle().ConfigureAwait(true);
			SettingsPage reloaded = Content<SettingsPage>(settings);
			Require(!ReferenceEquals(original, reloaded) && IsGeneralSelected(reloaded), "Fresh Settings content must restore General.");
			Require(!IsPaneOpen(reloaded) && !state.IsPaneOpen, "Closed pane must survive a language reload.");
			((NavigationView)original.FindName("SettingsNavigation")).IsPaneOpen = true;
			Require(!state.IsPaneOpen, "Discarded controls must no longer update the model.");
			Select(original, "AppearanceNavigationItem");
			Require(state.SelectedSection == SettingsSection.General, "Discarded selection bindings must not update the model.");
			state.SelectedSection = SettingsSection.Appearance;
			state.SelectedSection = SettingsSection.General;
			Require(!IsGeneralSelected(original), "Discarded pages must no longer observe the model.");
			Require(ReferenceEquals(options.GetState<SettingsWindowState>(), state) && state.SelectedSection == SettingsSection.General, "The same state instance must survive reload.");
			Require(ReferenceEquals(settings, GetWindow(WindowKey.Settings)) && ReferenceEquals(shell, settings.Content), "Window and shell must survive reload.");
			Require(settings.AppWindow.Id.Value == id.Value && settings.AppWindow.Size.Equals(bounds) && settings.AppWindow.Position.Equals(position), "Native identity and bounds must survive reload.");
			Require(((TextBlock)SelectedPage(reloaded).FindName("Localized1")).Text != oldLabel, "Restored General page must display the new language.");
			Require(GeneralSettings.IsEditing, "Restored General page must pause input.");
			Require(!ReferenceEquals(mainContent, Content<MainPage>(main)) && engine.IntervalMilliseconds == interval, "Stateless window registration must still reload normally.");
			settings.Close();
			await Settle().ConfigureAwait(true);
			Require(!GeneralSettings.IsEditing, "Closing General must release its input pause.");
			EasyWindows.ReloadAllWindows();
			Require(!Dictionary("WindowControllers").Contains(WindowKey.Settings), "Reload must keep closed windows closed.");
			EasyWindows.CreateWindow(WindowKey.Settings);
			await Settle().ConfigureAwait(true);
			settings = GetWindow(WindowKey.Settings);
			Require(IsGeneralSelected(Content<SettingsPage>(settings)) && ReferenceEquals(options.GetState<SettingsWindowState>(), state), "Reopening must reuse registration state.");
			Require(!IsPaneOpen(Content<SettingsPage>(settings)) && !state.IsPaneOpen, "Reopening must restore the closed pane.");
			Require(GeneralSettings.IsEditing, "Reopened General must pause input.");
			((NavigationView)Content<SettingsPage>(settings).FindName("SettingsNavigation")).IsPaneOpen = true;
			Require(state.IsPaneOpen, "Opening the pane must update the model.");
			Select(Content<SettingsPage>(settings), "AppearanceNavigationItem");
			for (int i = 0; i < 3; i++) {
				EasyWindows.ReloadAllWindows();
				await Settle().ConfigureAwait(true);
				Require(!IsGeneralSelected(Content<SettingsPage>(settings)) && state.SelectedSection == SettingsSection.Appearance, "Appearance must survive repeated reloads.");
				Require(!GeneralSettings.IsEditing, "Old General pages must not leave input paused.");
				Require(IsPaneOpen(Content<SettingsPage>(settings)) && state.IsPaneOpen, "Open pane must survive repeated reloads.");
			}
			settings.Close();
			await Settle().ConfigureAwait(true);
			EasyWindows.CreateWindow(WindowKey.Settings);
			await Settle().ConfigureAwait(true);
			Require(!IsGeneralSelected(Content<SettingsPage>(GetWindow(WindowKey.Settings))), "Reopening must remember the latest section, Appearance.");
			Require(IsPaneOpen(Content<SettingsPage>(GetWindow(WindowKey.Settings))) && state.IsPaneOpen, "Reopening must restore the open pane.");
			settings = GetWindow(WindowKey.Settings);
			settings.AppWindow.Resize(new Windows.Graphics.SizeInt32(500, 600));
			await Settle().ConfigureAwait(true);
			state.IsPaneOpen = true;
			EasyWindows.ReloadAllWindows();
			await Settle().ConfigureAwait(true);
			Require(state.IsPaneOpen && IsPaneOpen(Content<SettingsPage>(settings)), "Template initialization at compact width must retain an open pane.");
			result = "PASS: live settings do not write files, footer Save persists all settings, language reload retains edits, close discards changes, failed save preserves disk and baseline, saved settings reload; MainPage initialization, mouse selection, hold/enabled toggles, timing reciprocals/precision/bounds/empty input, disposed control isolation; observable model updates live navigation/pane/input pause, independent window state, model-backed selection and pane state, open/closed pane retained across reload and reopening (including compact width), detached controls cannot mutate or observe state, public state lookup/reference identity, missing/wrong state errors, read-only registrations, localization, native window identity/bounds, repeated reloads, input-pause cleanup, stateless registration compatibility.";
		}
		catch (Exception exception) { result = "FAIL: " + exception; }
		finally {
			if (Dictionary("WindowControllers").Contains(WindowKey.Settings)) GetWindow(WindowKey.Settings).Close();
			if (!Localization.TrySetLanguage(preference, out string error)) result = "FAIL: could not restore language preference: " + error;
			await Settle().ConfigureAwait(true);
		}
		await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "reload-verification.txt"), result).ConfigureAwait(true);
		GetWindow(WindowKey.Main).Close();
	}

	private static void VerifyRandomOffset() {
		var engine = new ClickEngine { IntervalMilliseconds = 100 };
		Require(engine.GetNextIntervalMilliseconds() == 100, "Zero offset must retain fixed timing.");
		using var page = new MainPage(engine);
		var offset = (Components.UnitNumberBox)page.FindName("RandomOffsetNumberBox");
		var input = (NumberBox)offset.FindName("InputNumberBox");
		input.Value = 20;
		Require(engine.RandomOffsetMilliseconds == 20 && engine.IntervalMilliseconds == 100, "Offset must update independently of base timing.");
		bool below = false, above = false;
		for (int i = 0; i < 10000; i++) {
			double interval = engine.GetNextIntervalMilliseconds();
			Require(interval >= 90 && interval <= 110, "100 ms with 20 ms offset must stay within 90–110 ms.");
			below |= interval < 100;
			above |= interval > 100;
		}
		Require(below && above, "Random timing must vary on both sides of the base delay.");
		input.Value = double.NaN;
		Require(engine.RandomOffsetMilliseconds == 20, "Empty offset must retain the previous value.");
		using var reloaded = new MainPage(engine);
		Require(((Components.UnitNumberBox)reloaded.FindName("RandomOffsetNumberBox")).Value == 20, "Reload must retain offset.");
		input.Value = 0;
		Require(engine.GetNextIntervalMilliseconds() == 100, "Zero must disable random timing.");
		page.Dispose();
		input.Value = 50;
		Require(engine.RandomOffsetMilliseconds == 0, "Disposed offset control must not mutate engine.");
		engine.IntervalMilliseconds = 1;
		engine.RandomOffsetMilliseconds = 1000000;
		for (int i = 0; i < 1000; i++) Require(engine.GetNextIntervalMilliseconds() >= 15, "Large offsets must not produce delays below 15 ms.");
	}

	private static void VerifyMainPageControls() {
		Windows.System.VirtualKey[] buttons = [Windows.System.VirtualKey.LeftButton, Windows.System.VirtualKey.RightButton,
			Windows.System.VirtualKey.MiddleButton, Windows.System.VirtualKey.XButton1, Windows.System.VirtualKey.XButton2];
		for (int index = 0; index < buttons.Length; index++) {
			// This engine is never run, so checking Enabled cannot inject clicks.
			var engine = new ClickEngine { MouseButton = buttons[index], HoldMode = true, Enabled = true, IntervalMilliseconds = 1000d / 3 };
			using var page = new MainPage(engine);
			ComboBox mouse = (ComboBox)page.FindName("MouseButtonComboBox");
			ToggleSwitch hold = (ToggleSwitch)page.FindName("HoldModeToggleSwitch");
			ToggleSwitch enabled = (ToggleSwitch)page.FindName("ClickingEnabledToggleSwitch");
			Components.UnitNumberBox delay = (Components.UnitNumberBox)page.FindName("DelayNumberBox");
			Components.UnitNumberBox cps = (Components.UnitNumberBox)page.FindName("CpsNumberBox");
			NumberBox delayInput = (NumberBox)delay.FindName("InputNumberBox");
			NumberBox cpsInput = (NumberBox)cps.FindName("InputNumberBox");
			Require(!delay.IsEnabled && !cps.IsEnabled && !mouse.IsEnabled && !hold.IsEnabled
				&& !((Components.UnitNumberBox)page.FindName("RandomOffsetNumberBox")).IsEnabled && enabled.IsEnabled,
				"Enabled clicking must lock all configuration while keeping stop available.");
			Require(mouse.SelectedIndex == index && hold.IsOn && enabled.IsOn && engine.Enabled,
				"MainPage must restore engine controls without changing enabled state.");
			Require(delay.Value == engine.IntervalMilliseconds && cps.Value == 3,
				"Initialization must retain exact timing despite rounded display values.");
			mouse.SelectedIndex = (index + 1) % buttons.Length;
			Require(engine.MouseButton == buttons[(index + 1) % buttons.Length], "Mouse selection must map every button correctly.");
			hold.IsOn = false;
			enabled.IsOn = false;
			Require(delay.IsEnabled && cps.IsEnabled && mouse.IsEnabled && hold.IsEnabled
				&& ((Components.UnitNumberBox)page.FindName("RandomOffsetNumberBox")).IsEnabled,
				"Stopping must unlock all configuration controls.");
			Require(!engine.HoldMode && !engine.Enabled, "Toggle changes must update the engine.");
			delayInput.Value = 250;
			Require(engine.IntervalMilliseconds == 250 && cps.Value == 4, "Delay edits must update CPS.");
			cpsInput.Value = 3;
			Require(engine.IntervalMilliseconds == 1000d / 3 && delay.Value == 1000d / 3,
				"CPS edits must retain an exact reciprocal instead of rounded milliseconds.");
			delayInput.Value = double.NaN;
			Require(engine.IntervalMilliseconds == 1000d / 3, "Empty delay input must not change engine timing.");
			cpsInput.Value = double.NaN;
			Require(engine.IntervalMilliseconds == 1000d / 3, "Empty CPS input must not change engine timing.");
			delayInput.Value = 1;
			Require(engine.IntervalMilliseconds == 15 && cps.Value == 1000d / 15, "Minimum delay must map to maximum CPS.");
			cpsInput.Value = 0.001;
			double maximumInterval = 1000d / cps.Minimum;
			Require(engine.IntervalMilliseconds == maximumInterval && delay.Value == maximumInterval, "Minimum CPS must retain the reciprocal of the actual XAML control bound.");
			page.Dispose();
			mouse.SelectedIndex = index;
			hold.IsOn = true;
			enabled.IsOn = true;
			delayInput.Value = 500;
			cpsInput.Value = 10;
			Require(engine.MouseButton == buttons[(index + 1) % buttons.Length] && !engine.HoldMode && !engine.Enabled
				&& engine.IntervalMilliseconds == maximumInterval, "Disposed controls must no longer change the engine.");
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1508", Justification = "Two-way XAML bindings update the state through control property changes.")]
	private static async Task VerifyIndependentWindow(SettingsWindowState firstState) {
		var key = new object();
		var secondState = new SettingsWindowState { SelectedSection = SettingsSection.General, IsPaneOpen = false };
		EasyWindows.RegisterWindow(key, new EasyWindows.WindowOptions<SettingsWindowState> {
			State = secondState,
			ContentFactory = state => new SettingsPage(state),
			Title = "Independent settings verification",
			DefaultSize = new Windows.Graphics.SizeInt32(860, 600)
		});
		EasyWindows.CreateWindow(key);
		Window secondWindow = GetWindow(key);
		try {
			await Settle().ConfigureAwait(true);
			SettingsPage secondPage = Content<SettingsPage>(secondWindow);
			Require(IsGeneralSelected(secondPage) && !IsPaneOpen(secondPage), "A second registration must use its own initial state.");
			secondState.SelectedSection = SettingsSection.Appearance;
			secondState.IsPaneOpen = true;
			await Settle().ConfigureAwait(true);
			Require(!IsGeneralSelected(secondPage) && IsPaneOpen(secondPage), "A second state's changes must update its own page.");
			Select(secondPage, "GeneralNavigationItem");
			((NavigationView)secondPage.FindName("SettingsNavigation")).IsPaneOpen = false;
			Require(secondState.SelectedSection == SettingsSection.General && !secondState.IsPaneOpen,
				"The second page must write back to its own model.");
			Require(firstState.SelectedSection == SettingsSection.Appearance && firstState.IsPaneOpen,
				"Changes in the second window must not affect the first state.");
		}
		finally { secondWindow.Close(); }
		await Settle().ConfigureAwait(true);
		Require(!GeneralSettings.IsEditing, "Closing the independent General page must release its input pause.");
	}

    private static async Task VerifySettingsPersistence() {
        string path = SettingsStore.FilePath;
        byte[]? original = File.Exists(path) ? File.ReadAllBytes(path) : null;
        var toggle = GeneralSettings.ToggleKey;
        var hold = GeneralSettings.HoldKey;
        string language = GeneralSettings.Language;
        var theme = GamiToolkit.Settings.ThemeSettingsStore.FromCurrentTheme();
        string legacyLanguage = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GamiAutoClicker", "language.txt");
        byte[]? oldLanguage = File.Exists(legacyLanguage) ? File.ReadAllBytes(legacyLanguage) : null;
        string themePath = GamiToolkit.Settings.ThemeSettingsStore.FilePath;
        byte[]? oldTheme = File.Exists(themePath) ? File.ReadAllBytes(themePath) : null;
        try {
            EasyWindows.CreateWindow(WindowKey.Settings);
            var nextKey = toggle == Windows.System.VirtualKey.F9 ? Windows.System.VirtualKey.F10 : Windows.System.VirtualKey.F9;
            string nextLanguage = language == "pl" ? "en" : "pl";
            Require(GeneralSettings.TryApply(nextKey, Windows.System.VirtualKey.XButton2, nextLanguage, out _), "Live settings must apply.");
            Gami.EasyWindows.ApplyTheme(Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light);
            await Settle().ConfigureAwait(true);
            Require(GeneralSettings.ToggleKey == nextKey && GeneralSettings.Language == nextLanguage, "Reload must preserve live edits.");
            Require(SameBytes(path, original) && SameBytes(legacyLanguage, oldLanguage) && SameBytes(themePath, oldTheme), "Live edits must not write any settings file.");
            GetWindow(WindowKey.Settings).Close();
            await Settle().ConfigureAwait(true);
            Require(GeneralSettings.ToggleKey == toggle && GeneralSettings.HoldKey == hold && GeneralSettings.Language == language
                && (int)Gami.EasyWindows.Theme.Theme == theme.Theme, "Closing must discard all unsaved settings.");
            EasyWindows.CreateWindow(WindowKey.Settings);
            Require(GeneralSettings.TryApply(nextKey, Windows.System.VirtualKey.XButton2, nextLanguage, out _), "Saved settings must apply.");
            Gami.EasyWindows.ApplyTheme(Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light);
            await Settle().ConfigureAwait(true);
            SettingsPage page = Content<SettingsPage>(GetWindow(WindowKey.Settings));
            typeof(SettingsPage).GetMethod("SaveSettings", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(page, null);
            Require(!((InfoBar)page.FindName("SaveError")).IsOpen && File.Exists(path), "Footer Save must persist successfully.");
            byte[] saved = File.ReadAllBytes(path);
            GeneralSettings.TryApply(toggle, hold, language, out _);
            Gami.EasyWindows.ApplyTheme(Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark);
            await Settle().ConfigureAwait(true);
            using (var lockedFile = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.None)) {
                Require(!SettingsStore.TrySave(out string error) && error.Length > 0, "Save failure must be reported.");
                Require(GeneralSettings.Language == language, "Save failure must retain live edits.");
            }
            Require(SameBytes(path, saved), "Failed save must preserve persisted bytes.");
            GetWindow(WindowKey.Settings).Close();
            await Settle().ConfigureAwait(true);
            Require(GeneralSettings.ToggleKey == nextKey && GeneralSettings.Language == nextLanguage
                && Gami.EasyWindows.Theme.Theme == Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Light,
                "Closing after a failed save must restore the last successful save.");
            GeneralSettings.TryApply(toggle, hold, language, out _);
            SettingsStore.Load();
            await Settle().ConfigureAwait(true);
            Require(GeneralSettings.ToggleKey == nextKey && GeneralSettings.HoldKey == Windows.System.VirtualKey.XButton2
                && GeneralSettings.Language == nextLanguage, "Loading must restore saved settings.");
        }
        finally {
            if (Dictionary("WindowControllers").Contains(WindowKey.Settings)) GetWindow(WindowKey.Settings).Close();
            if (original is null) File.Delete(path); else File.WriteAllBytes(path, original);
            File.Delete(path + ".tmp");
            GeneralSettings.TryApply(toggle, hold, language, out _);
            GamiToolkit.Settings.ThemeSettingsStore.Apply(theme);
            SettingsStore.Load();
            await Settle().ConfigureAwait(true);
        }
    }

    private static bool SameBytes(string path, byte[]? expected) => expected is null
        ? !File.Exists(path) : File.Exists(path) && System.Linq.Enumerable.SequenceEqual(File.ReadAllBytes(path), expected);

	private static IDictionary Dictionary(string property) => (IDictionary)typeof(EasyWindows).GetProperty(property, BindingFlags.Static | BindingFlags.NonPublic)!.GetValue(null)!;
	private static Window GetWindow(object key) {
		object controller = Dictionary("WindowControllers")[key]!;
		return (Window)controller.GetType().GetProperty("Window")!.GetValue(controller)!;
	}
	private static T Content<T>(Window window) => (T)((ContentControl)((Grid)window.Content).Children[1]).Content;
	private static bool IsGeneralSelected(SettingsPage page) => ReferenceEquals(((NavigationView)page.FindName("SettingsNavigation")).SelectedItem, page.FindName("GeneralNavigationItem"));
	private static bool IsPaneOpen(SettingsPage page) => ((NavigationView)page.FindName("SettingsNavigation")).IsPaneOpen;
	private static Page SelectedPage(SettingsPage page) => (Page)((ContentControl)page.FindName("SettingsContent")).Content;
	private static void Select(SettingsPage page, string item) => ((NavigationView)page.FindName("SettingsNavigation")).SelectedItem = page.FindName(item);
	private static Task Settle() => Task.Delay(200);
	private static void Expect<TException>(Action action) where TException : Exception {
		try { action(); }
		catch (TException) { return; }
		throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
	}
	private static void Require(bool condition, string message) {
		if (!condition) throw new InvalidOperationException(message);
	}
}
