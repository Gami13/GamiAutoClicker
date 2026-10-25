using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;
using GamiToolkit.Components;
using static GamiToolkit.Components.SettingsControls;

namespace GamiAutoClicker;

internal sealed class AppearancePage : ResponsiveComponent {
	private static readonly EasyWindows.BackdropMaterial[] Materials = Enum.GetValues<EasyWindows.BackdropMaterial>();
	private static readonly SystemBackdropTheme[] Themes =
		[SystemBackdropTheme.Dark, SystemBackdropTheme.Light, SystemBackdropTheme.Default];

	public override Element Render() {
		double width = UseContentWidth();
		bool wide = width >= 590;
		bool colorsWide = width >= 820;
		var (_, refresh) = UseState(0);
		UseEffect(() => {
			int version = 0;
			void Changed(object? sender, EventArgs args) => refresh(++version);
			EasyWindows.ThemeChanged += Changed;
			return () => EasyWindows.ThemeChanged -= Changed;
		}, []);

		EasyWindows.ThemeSettings current = EasyWindows.Theme;
		string[] materialLabels = Materials.Select(FormatMaterial).ToArray();
		string[] themeLabels = Themes.Select(theme => Localization.Get(theme.ToString())).ToArray();

		return Grid([GridSize.Star()], [GridSize.Auto, GridSize.Star()],
			Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
				VStack(2,
					Subtitle(Localization.Get("AppearancePage_Localized1.Text")),
					SettingsCaption(Localization.Get("AppearancePage_Localized2.Text"))).Grid(0, 0),
				RestoreDefaultsButton(Localization.Get("Restoredefaults"), width >= 470, EasyWindows.RestoreThemeDefaults).Grid(0, 1)
			).Margin(new Microsoft.UI.Xaml.Thickness(24, 16, 24, 12)).Set(g => g.ColumnSpacing = 16),
			ScrollView(Grid([GridSize.Star(), wide ? GridSize.Star() : GridSize.Px(0)], [GridSize.Auto, GridSize.Auto, GridSize.Auto],
				SettingsCard(
					HStack(8, TextBlock("\uE790").FontFamily("Segoe Fluent Icons").FontSize(18).Foreground(new ThemeRef("AccentTextFillColorPrimaryBrush")), BodyStrong(Localization.Get("AppearancePage_Localized3.Text"))),
					SettingsCaption(Localization.Get("AppearancePage_Localized4.Text")),
					Border(VStack(3, BodyStrong(FormatMaterial(current.BackdropMaterial)),
					SettingsCaption(current.Theme == SystemBackdropTheme.Default
						? Localization.Get("FollowsWindowscolormode")
						: Localization.Format("ColorModeSummary", Localization.Get(current.Theme.ToString()))),
					SettingsCaption(Localization.Get(current.ShouldOverride
						? "Customcolortuningisactive" : "Usingsystemmaterialdefaults"))
					)).Padding(10).Background(new ThemeRef("LayerFillColorDefaultBrush")).BorderBrush(new ThemeRef("CardStrokeColorDefaultBrush")).BorderThickness(1).CornerRadius(8)
				).Grid(0, 0),
				SettingsCard(
					BodyStrong(Localization.Get("AppearancePage_Localized5.Text")),
					SettingsCaption(Localization.Get("AppearancePage_Localized6.Text")),
					ComboBox(materialLabels, Array.IndexOf(Materials, current.BackdropMaterial), index => {
						if ((uint)index < Materials.Length) EasyWindows.SetBackdropMaterial(Materials[index]);
					}).AutomationName(Localization.Get("AppearancePage_BackdropMaterialComboBox.Header")).Set(c => c.Header = Localization.Get("AppearancePage_BackdropMaterialComboBox.Header")),
					SettingsCaption(Localization.Get("AppearancePage_Localized7.Text")),
					ComboBox(themeLabels, Array.IndexOf(Themes, current.Theme), index => {
						if ((uint)index < Themes.Length) EasyWindows.ApplyTheme(Themes[index]);
					}).AutomationName(Localization.Get("AppearancePage_ThemeComboBox.Header")).Set(c => c.Header = Localization.Get("AppearancePage_ThemeComboBox.Header")),
					SettingsCaption(Localization.Get("AppearancePage_Localized8.Text"))
				).Grid(1, 0),
				SettingsCard(
					BodyStrong(Localization.Get("AppearancePage_Localized9.Text")),
					SettingsCaption(Localization.Get("AppearancePage_Localized10.Text")),
					ToggleSwitch(current.ShouldOverride, EasyWindows.SetOverrides,
						header: Localization.Get("AppearancePage_OverrideDefaultsToggleSwitch.Header")),
					VStack(10,
						Grid([GridSize.Star(), colorsWide ? GridSize.Star() : GridSize.Px(0)], [GridSize.Auto, GridSize.Auto],
						ColorControl(Localization.Get("AppearancePage_FallbackColorPicker.Header"),
							current.FallbackColor, EasyWindows.SetFallbackColor).Grid(0, 0),
						ColorControl(Localization.Get("AppearancePage_TintColorPicker.Header"),
							current.TintColor, EasyWindows.SetTintColor).Grid(colorsWide ? 0 : 1, colorsWide ? 1 : 0)
						).Set(g => { g.ColumnSpacing = 10; g.RowSpacing = 10; }),
						PercentageControl(Localization.Get("AppearancePage_TintOpacitySlider.Header"),
							current.TintOpacity, value => EasyWindows.SetTintOpacity((float)value)),
						PercentageControl(Localization.Get("AppearancePage_LuminosityOpacitySlider.Header"),
							current.LuminosityOpacity, value => EasyWindows.SetLuminosityOpacity((float)value))
					).IsEnabled(current.ShouldOverride).Opacity(current.ShouldOverride ? 1 : 0.55)
				).Grid(wide ? 0 : 2, wide ? 1 : 0, wide ? 2 : 1, 1)
			).Margin(new Microsoft.UI.Xaml.Thickness(24, 0, 24, 8)).Set(g => { g.ColumnSpacing = 12; g.RowSpacing = 12; })).Set(view => {
				view.HorizontalScrollMode = Microsoft.UI.Xaml.Controls.ScrollingScrollMode.Disabled;
				view.HorizontalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollingScrollBarVisibility.Hidden;
				view.VerticalScrollBarVisibility = Microsoft.UI.Xaml.Controls.ScrollingScrollBarVisibility.Auto;
			}).Grid(1, 0)
		).Set(CaptureRoot);
	}

	private static Element ColorControl(string label, Color color, Action<Color> changed) => ColorPickerFlyoutButton.Create(label, color, changed);
	private static Element PercentageControl(string label, double value, Action<double> changed) => LabeledPercentageSlider(label, value, changed);

	private static string FormatMaterial(EasyWindows.BackdropMaterial material) => material switch {
		EasyWindows.BackdropMaterial.MicaAlt => Localization.Get("MicaAlt"),
		EasyWindows.BackdropMaterial.AcrylicThin => Localization.Get("ThinAcrylic"),
		_ => Localization.Get(material.ToString())
	};
}
