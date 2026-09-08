using System;
using System.Linq;
using Windows.System;

namespace GamiAutoClicker;

internal static class GeneralSettings
{
    public static readonly VirtualKey[] Keys = Enum.GetValues<VirtualKey>().Where(key =>
        key is VirtualKey.LeftButton or VirtualKey.RightButton or VirtualKey.MiddleButton or VirtualKey.XButton1 or VirtualKey.XButton2
        or VirtualKey.Space or VirtualKey.Insert or VirtualKey.Delete or VirtualKey.Home or VirtualKey.End or VirtualKey.PageUp or VirtualKey.PageDown
        or VirtualKey.LeftShift or VirtualKey.RightShift or VirtualKey.LeftControl or VirtualKey.RightControl or VirtualKey.LeftMenu or VirtualKey.RightMenu
        || ((int)key >= (int)VirtualKey.A && (int)key <= (int)VirtualKey.Z)
        || ((int)key >= (int)VirtualKey.Number0 && (int)key <= (int)VirtualKey.Number9)
        || ((int)key >= (int)VirtualKey.F1 && (int)key <= (int)VirtualKey.F24)).Distinct().ToArray();

    public static VirtualKey ToggleKey { get; private set; } = VirtualKey.F8;
    public static VirtualKey HoldKey { get; private set; } = VirtualKey.XButton1;
    public static string Language { get; private set; } = "system";
    public static bool IsEditing { get; set; }
    public static event Action? Changed;

    public static bool TryApply(VirtualKey toggle, VirtualKey hold, string language, out string error)
    {
        error = "";
        if (!Keys.Contains(toggle) || !Keys.Contains(hold)) { error = "Choose a supported key or mouse button."; return false; }
        if (toggle == hold) { error = "Choose different buttons for toggle clicking and hold to click."; return false; }
        ToggleKey = toggle;
        HoldKey = hold;
        Language = language;
        Changed?.Invoke();
        return true;
    }

    public static string KeyLabel(VirtualKey key) => key switch
    {
        VirtualKey.LeftButton => "Mouse 1 (left)", VirtualKey.RightButton => "Mouse 2 (right)",
        VirtualKey.MiddleButton => "Mouse 3 (middle)", VirtualKey.XButton1 => "Mouse 4 (back)", VirtualKey.XButton2 => "Mouse 5 (forward)",
        VirtualKey.LeftShift => "Left Shift", VirtualKey.RightShift => "Right Shift",
        VirtualKey.LeftControl => "Left Ctrl", VirtualKey.RightControl => "Right Ctrl",
        VirtualKey.LeftMenu => "Left Alt", VirtualKey.RightMenu => "Right Alt",
        VirtualKey.PageUp => "Page Up", VirtualKey.PageDown => "Page Down",
        _ => key.ToString().Replace("Number", "", StringComparison.Ordinal)
    };
}
