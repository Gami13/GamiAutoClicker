using Gami;
using GamiAutoClicker;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Automation;
using Windows.Graphics;
using Windows.Globalization.NumberFormatting;
using Windows.System;
using static Microsoft.UI.Reactor.Factories;
using AppWindowKey = GamiAutoClicker.WindowKey;
using NativeGrid = Microsoft.UI.Xaml.Controls.Grid;
using NativeButton = Microsoft.UI.Xaml.Controls.Button;
using NativeText = Microsoft.UI.Xaml.Controls.TextBlock;

// Exercise persistence in a writable, isolated file. The application default is unchanged.
string reportPath = Environment.GetEnvironmentVariable("GAMI_VERIFICATION_REPORT") ?? Path.Combine(AppContext.BaseDirectory, "verification.txt");
SettingsStore.FilePath = Path.Combine(Path.GetDirectoryName(reportPath)!, "verification-settings.bin");
AppState.Initialize();
ReactorApp.Run<VerificationRoot>(EasyWindows.GetWindowSpec(AppWindowKey.Main));

sealed class VerificationRoot : Component {
	public override Element Render() {
		var window = UseWindow()!;
		UseEffect(() => { _ = RunAsync(window); return () => { }; }, []);
		return Component<MainWindow>();
	}
	private static async Task RunAsync(ReactorWindow main) {
		string report = Environment.GetEnvironmentVariable("GAMI_VERIFICATION_REPORT") ?? Path.Combine(AppContext.BaseDirectory, "verification.txt");
		var results = new List<string>();
		string path = SettingsStore.FilePath;
		byte[]? backup = File.Exists(path) ? File.ReadAllBytes(path) : null;
		void Check(bool ok, string message) { if (!ok) throw new InvalidOperationException(message); results.Add("PASS " + message); }
		try {
			await Task.Delay(700);
			main.AppWindow.Hide();
			var root = (FrameworkElement)main.NativeWindow.Content;
			Check(!AppState.Engine.Enabled, "clicking stays disabled throughout verification");
			Check(main.AppWindow.Size.Width == 370 && main.AppWindow.Size.Height == 370, "original physical window bounds");
			var numbers = All<Microsoft.UI.Xaml.Controls.NumberBox>(root).ToArray();
			Check(numbers.Length == 3, "three unit number boxes");
			var mouseSelector = All<Microsoft.UI.Xaml.Controls.ComboBox>(root).Single();
			var statusToggle = All<Microsoft.UI.Xaml.Controls.ToggleSwitch>(root).Single(toggle => AutomationProperties.GetName(toggle) == Localization.Get("Enableclicking"));
			double timingY = numbers[0].TransformToVisual(root).TransformPoint(new Windows.Foundation.Point()).Y;
			double inputY = mouseSelector.TransformToVisual(root).TransformPoint(new Windows.Foundation.Point()).Y;
			double statusY = statusToggle.TransformToVisual(root).TransformPoint(new Windows.Foundation.Point()).Y;
			Check(timingY < inputY && inputY < statusY, "extracted main sections retain their vertical layout");
			Check(numbers.All(n => n.SpinButtonPlacementMode == NumberBoxSpinButtonPlacementMode.Inline), "inline number spinners");
			Check(numbers[0].SmallChange == 1 && numbers[1].SmallChange == 0.001 && numbers[2].SmallChange == 1, "original number-box steps");
			Check(((DecimalFormatter)numbers[1].NumberFormatter).FractionDigits == 3, "CPS precision");
			Check(All<NativeButton>(root).Any(b => b.Content is SymbolIcon { Symbol: Symbol.Setting }), "settings icon in title bar");
			var title = All<NativeText>(root).Single(t => t.Text == "Gami's AutoClicker");
			var expectedCaption = new NativeText { Style = (Style)Application.Current.Resources["CaptionTextBlockStyle"] };
			Check(title.FontFamily.Source == expectedCaption.FontFamily.Source && title.FontSize == expectedCaption.FontSize && title.FontWeight.Weight == expectedCaption.FontWeight.Weight && title.TextLineBounds == expectedCaption.TextLineBounds, "native caption typography matches original");
			var titleGrid = (NativeGrid)VisualTreeHelper.GetParent(title);
			Check(titleGrid.ColumnSpacing == 2, "original title-bar column spacing");
			var iconButton = titleGrid.Children.OfType<NativeButton>().Single();
			var expectedBorder = (Thickness)Application.Current.Resources["ButtonBorderThemeThickness"];
			Check(iconButton.BorderThickness == expectedBorder && iconButton.TransformToVisual(titleGrid).TransformPoint(new Windows.Foundation.Point()).X >= 0.5, "original icon button border and left inset (actual=" + iconButton.BorderThickness + ", expected=" + expectedBorder + ", x=" + iconButton.TransformToVisual(titleGrid).TransformPoint(new Windows.Foundation.Point()).X + ")");
			numbers[1].Value = 3.333;
			await Task.Delay(200);
			Check(Math.Abs(AppState.Engine.IntervalMilliseconds - 1000d / 3.333) < 0.001, "CPS edits retain engine precision");
			AppState.OpenSettings(); await Task.Delay(600);
			var settings = EasyWindows.GetWindow(AppWindowKey.Settings);
			if (settings is null) throw new InvalidOperationException("Settings registration missing: " + string.Join(" | ", ReactorApp.Windows.Select(w => w.Spec.Key + ": " + string.Join("; ", All<NativeText>(w.NativeWindow.Content).Select(t => t.Text)))));
			settings.AppWindow.Hide();
			var settingsRoot = (FrameworkElement)settings.NativeWindow.Content;
			var nav = All<Microsoft.UI.Xaml.Controls.NavigationView>(settingsRoot).Single();
			Check(nav.FooterMenuItems.Count == 1 && nav.FooterMenuItems[0] is NavigationViewItem { SelectsOnInvoked: false }, "Save is a non-selecting footer menu item");
			AppState.OpenSettings(); await Task.Delay(100); settings.AppWindow.Hide();
			Check(ReferenceEquals(settings, EasyWindows.GetWindow(AppWindowKey.Settings)), "opening settings reuses the same window");
			EasyWindows.ApplyTheme(Microsoft.UI.Composition.SystemBackdrops.SystemBackdropTheme.Dark);
			EasyWindows.SetOverrides(true);
			EasyWindows.SetTintOpacity(0.42f);
			await Task.Delay(200);
			Check(settingsRoot.RequestedTheme == ElementTheme.Dark && root.RequestedTheme == ElementTheme.Dark, "theme applies to both windows");
			var colorButtons = All<NativeButton>(settingsRoot).Where(b => b.Flyout is Microsoft.UI.Xaml.Controls.Flyout { Content: Microsoft.UI.Xaml.Controls.ColorPicker }).ToArray();
			Check(colorButtons.Length == 2, "two reusable color flyout controls: " + (colorButtons.Length == 2 ? "present" : string.Join(" | ", All<NativeText>(settingsRoot).Select(t => t.Text))));
			Check(colorButtons.All(b => ((Microsoft.UI.Xaml.Controls.Flyout)b.Flyout!).SystemBackdrop is EasyWindowsBackdrop), "custom flyout backdrops");
			Check(colorButtons.All(b => ((Microsoft.UI.Xaml.Controls.ColorPicker)((Microsoft.UI.Xaml.Controls.Flyout)b.Flyout!).Content).ColorSpectrumShape == ColorSpectrumShape.Ring), "ring color picker");
			var picker = (Microsoft.UI.Xaml.Controls.ColorPicker)((Microsoft.UI.Xaml.Controls.Flyout)colorButtons[0].Flyout!).Content;
			var chosenColor = Windows.UI.Color.FromArgb(160, 40, 80, 120);
			picker.Color = chosenColor; await Task.Delay(100);
			Check(EasyWindows.Theme.FallbackColor == chosenColor, "color edits update shared appearance");
			Check(All<Microsoft.UI.Xaml.Controls.Slider>(settingsRoot).All(slider => slider.StepFrequency == 0.01), "one percent opacity steps");
			// Check placement against actual viewport width at both ends of the supported range.
			foreach (int width in new[] { 1110, 434 }) {
				settings.AppWindow.Resize(new SizeInt32(width, 700)); await Task.Delay(300);
				var layout = All<NativeGrid>(settingsRoot).Single(g => g.ColumnSpacing == 12 && g.RowDefinitions.Count == 3);
				DependencyObject? ancestor = VisualTreeHelper.GetParent(layout);
				while (ancestor is not NativeGrid { RowDefinitions.Count: 2 } grid || !grid.Children.OfType<NativeGrid>().Any(child => child.ColumnSpacing == 16))
					ancestor = ancestor is null ? throw new InvalidOperationException("Page viewport not found") : VisualTreeHelper.GetParent(ancestor);
				var page = (FrameworkElement)ancestor;
				var advanced = layout.Children.OfType<Microsoft.UI.Xaml.Controls.Border>().Last();
				Check(NativeGrid.GetColumn(advanced) == (page.ActualWidth >= 590 ? 1 : 0), $"appearance column follows viewport at {width}px");
				var restore = All<NativeButton>(settingsRoot).Single(b => AutomationProperties.GetName(b) == Localization.Get("Restoredefaults"));
				Check((restore.Width == 38) == (page.ActualWidth < 470), $"restore button follows viewport at {width}px");
			}
			settings.AppWindow.Resize(new SizeInt32(860, 600));
			nav.SelectedItem = nav.MenuItems[0]; await Task.Delay(250);
			Check(GeneralSettings.IsEditing, "General page pauses hotkeys");
			Check(!GeneralSettings.TryApply(VirtualKey.F8, VirtualKey.F8, "en", out _), "duplicate hotkeys are rejected");
			var generalCombos = All<Microsoft.UI.Xaml.Controls.ComboBox>(settingsRoot).ToArray();
			generalCombos[1].SelectedIndex = Array.IndexOf(GeneralSettings.Keys, GeneralSettings.ToggleKey); await Task.Delay(100);
			Check(generalCombos[1].SelectedIndex == Array.IndexOf(GeneralSettings.Keys, GeneralSettings.HoldKey), "invalid key selection restores the model value");
			All<Microsoft.UI.Xaml.Controls.InfoBar>(settingsRoot).Single().IsOpen = false;
			generalCombos[1].SelectedIndex = Array.IndexOf(GeneralSettings.Keys, GeneralSettings.ToggleKey); await Task.Delay(100);
			Check(All<Microsoft.UI.Xaml.Controls.InfoBar>(settingsRoot).Single().IsOpen && generalCombos[1].SelectedIndex == Array.IndexOf(GeneralSettings.Keys, GeneralSettings.HoldKey), "repeated validation errors reopen after dismissal");
			generalCombos[0].SelectedIndex = Array.IndexOf(GeneralSettings.Keys, VirtualKey.F9); await Task.Delay(100);
			var enabledToggle = All<Microsoft.UI.Xaml.Controls.ToggleSwitch>(root).Single(t => AutomationProperties.GetName(t) == Localization.Get("Enableclicking"));
			Check((string?)ToolTipService.GetToolTip(enabledToggle) == Localization.Format("ToggleHint", GeneralSettings.KeyLabel(VirtualKey.F9)), "hotkey hints refresh while disabled");
			generalCombos[1].SelectedIndex = Array.IndexOf(GeneralSettings.Keys, VirtualKey.XButton2); await Task.Delay(100);
			generalCombos[2].SelectedIndex = Array.IndexOf(Localization.Languages, "pl"); await Task.Delay(250);
			Check(GeneralSettings.ToggleKey == VirtualKey.F9 && GeneralSettings.HoldKey == VirtualKey.XButton2 && GeneralSettings.Language == "pl", "General controls apply keys and language");
			Check(main.NativeWindow.Title == "Gami's AutoClicker" && settings.NativeWindow.Title == Localization.Get("Settings"), "localized window titles");
			Check(All<NativeText>(root).Any(t => t.Text == Localization.Get("MainWindow_Localized1.Text")), "main controls update language");
			Check(SettingsStore.TrySave(out _), "explicit Save persists settings");
			GeneralSettings.TryApply(VirtualKey.F10, VirtualKey.XButton1, "en", out _);
			nav.IsPaneOpen = false; await Task.Delay(100);
			settings.NativeWindow.Close(); await Task.Delay(200);
			Check(!GeneralSettings.IsEditing, "closing settings releases hotkey pause");
			Check(GeneralSettings.ToggleKey == VirtualKey.F9 && GeneralSettings.Language == "pl", "closing discards unsaved changes");
			AppState.OpenSettings(); await Task.Delay(400);
			var reopened = EasyWindows.GetWindow(AppWindowKey.Settings)!; reopened.AppWindow.Hide();
			Check(!ReferenceEquals(settings, reopened), "closed settings creates a fresh native window");
			Check(AppState.SettingsWindow.SelectedSection == SettingsSection.General && GeneralSettings.IsEditing, "selected page survives reopening");
			var reopenedNav = All<Microsoft.UI.Xaml.Controls.NavigationView>(reopened.NativeWindow.Content).Single();
			Check(!reopenedNav.IsPaneOpen, "pane state survives reopening");
			double interval = AppState.Engine.IntervalMilliseconds;
			EasyWindows.ReloadAllWindows(); await Task.Delay(300);
			Check(ReferenceEquals(reopened, EasyWindows.GetWindow(AppWindowKey.Settings)) && AppState.Engine.IntervalMilliseconds == interval, "content reload retains windows and engine state");
			reopened.NativeWindow.Close(); await Task.Delay(150);
			GeneralSettings.TryApply(VirtualKey.F10, VirtualKey.XButton1, "en", out _);
			SettingsStore.Load();
			Check(GeneralSettings.ToggleKey == VirtualKey.F9 && GeneralSettings.Language == "pl", "saved settings reload from disk");
			Check(!AppState.Engine.Enabled, "verification did not enable clicking");
			results.Add("SUCCESS");
		}
		catch (Exception error) { Environment.ExitCode = 1; results.Add("FAIL " + error); }
		finally {
			var settings = EasyWindows.GetWindow(AppWindowKey.Settings); settings?.NativeWindow.Close();
			if (backup is not null) File.WriteAllBytes(path, backup); else if (File.Exists(path)) File.Delete(path);
			File.WriteAllLines(report, results);
			main.NativeWindow.Close();
		}
	}
	private static IEnumerable<T> All<T>(DependencyObject root) where T : DependencyObject {
		if (root is T match) yield return match;
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++) foreach (T child in All<T>(VisualTreeHelper.GetChild(root, i))) yield return child;
	}
}
