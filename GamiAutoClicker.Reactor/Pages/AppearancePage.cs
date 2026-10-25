using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Reactor;
using Microsoft.UI.Reactor.Core;
using Windows.UI;
using static Microsoft.UI.Reactor.Factories;

namespace GamiAutoClicker;

internal sealed class AppearancePage : Component
{
    private static readonly EasyWindows.BackdropMaterial[] Materials = Enum.GetValues<EasyWindows.BackdropMaterial>();
    private static readonly SystemBackdropTheme[] Themes =
        [SystemBackdropTheme.Dark, SystemBackdropTheme.Light, SystemBackdropTheme.Default];

    public override Element Render()
    {
        var (_, refresh) = UseState(0);
        UseEffect(() =>
        {
            int version = 0;
            void Changed(object? sender, EventArgs args) => refresh(++version);
            EasyWindows.ThemeChanged += Changed;
            return () => EasyWindows.ThemeChanged -= Changed;
        }, []);

        EasyWindows.ThemeSettings current = EasyWindows.Theme;
        string[] materialLabels = Materials.Select(FormatMaterial).ToArray();
        string[] themeLabels = Themes.Select(theme => Localization.Get(theme.ToString())).ToArray();

        return VStack(12,
            Grid([GridSize.Star(), GridSize.Auto], [GridSize.Auto],
                VStack(2,
                    Subtitle(Localization.Get("AppearancePage_Localized1.Text")),
                    Caption(Localization.Get("AppearancePage_Localized2.Text"))).Grid(0, 0),
                Button(Localization.Get("Restoredefaults"), EasyWindows.RestoreThemeDefaults)
                    .AutomationName(Localization.Get("Restoredefaults")).Grid(0, 1)),
            ScrollView(VStack(12,
                Card(VStack(6,
                    BodyStrong(Localization.Get("AppearancePage_Localized3.Text")),
                    Caption(Localization.Get("AppearancePage_Localized4.Text")),
                    BodyStrong(FormatMaterial(current.BackdropMaterial)),
                    Caption(current.Theme == SystemBackdropTheme.Default
                        ? Localization.Get("FollowsWindowscolormode")
                        : Localization.Format("ColorModeSummary", Localization.Get(current.Theme.ToString()))),
                    Caption(Localization.Get(current.ShouldOverride
                        ? "Customcolortuningisactive" : "Usingsystemmaterialdefaults"))
                )),
                Card(VStack(8,
                    BodyStrong(Localization.Get("AppearancePage_Localized5.Text")),
                    Caption(Localization.Get("AppearancePage_Localized6.Text")),
                    TextBlock(Localization.Get("AppearancePage_BackdropMaterialComboBox.Header")),
                    ComboBox(materialLabels, Array.IndexOf(Materials, current.BackdropMaterial), index =>
                    {
                        if ((uint)index < Materials.Length) EasyWindows.SetBackdropMaterial(Materials[index]);
                    }).AutomationName(Localization.Get("AppearancePage_BackdropMaterialComboBox.Header")),
                    Caption(Localization.Get("AppearancePage_Localized7.Text")),
                    TextBlock(Localization.Get("AppearancePage_ThemeComboBox.Header")),
                    ComboBox(themeLabels, Array.IndexOf(Themes, current.Theme), index =>
                    {
                        if ((uint)index < Themes.Length) EasyWindows.ApplyTheme(Themes[index]);
                    }).AutomationName(Localization.Get("AppearancePage_ThemeComboBox.Header")),
                    Caption(Localization.Get("AppearancePage_Localized8.Text"))
                )),
                Card(VStack(10,
                    BodyStrong(Localization.Get("AppearancePage_Localized9.Text")),
                    Caption(Localization.Get("AppearancePage_Localized10.Text")),
                    ToggleSwitch(current.ShouldOverride, EasyWindows.SetOverrides,
                        header: Localization.Get("AppearancePage_OverrideDefaultsToggleSwitch.Header")),
                    Border(VStack(10,
                        ColorControl(Localization.Get("AppearancePage_FallbackColorPicker.Header"),
                            current.FallbackColor, EasyWindows.SetFallbackColor),
                        ColorControl(Localization.Get("AppearancePage_TintColorPicker.Header"),
                            current.TintColor, EasyWindows.SetTintColor),
                        PercentageControl(Localization.Get("AppearancePage_TintOpacitySlider.Header"),
                            current.TintOpacity, value => EasyWindows.SetTintOpacity((float)value)),
                        PercentageControl(Localization.Get("AppearancePage_LuminosityOpacitySlider.Header"),
                            current.LuminosityOpacity, value => EasyWindows.SetLuminosityOpacity((float)value))
                    )).IsEnabled(current.ShouldOverride).Opacity(current.ShouldOverride ? 1 : 0.55)
                ))
            ))
        ).Padding(20);
    }

    private static StackElement ColorControl(string label, Color color, Action<Color> changed) =>
        VStack(4,
            TextBlock(label),
            Flyout(Button($"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}")
                    .AutomationName($"{label}: #{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}"),
                ColorPicker(color, changed).Set(picker =>
                {
                    picker.IsAlphaEnabled = true;
                    picker.IsColorChannelTextInputVisible = true;
                    picker.IsHexInputVisible = true;
                    picker.IsMoreButtonVisible = true;
                })));

    private static StackElement PercentageControl(string label, double value, Action<double> changed) =>
        VStack(4,
            HStack(8, TextBlock(label), TextBlock($"{Math.Round(value * 100):0}%")),
            Slider(value, 0, 1, changed).AutomationName(label));

    private static string FormatMaterial(EasyWindows.BackdropMaterial material) => material switch
    {
        EasyWindows.BackdropMaterial.MicaAlt => Localization.Get("MicaAlt"),
        EasyWindows.BackdropMaterial.AcrylicThin => Localization.Get("ThinAcrylic"),
        _ => Localization.Get(material.ToString())
    };
}
