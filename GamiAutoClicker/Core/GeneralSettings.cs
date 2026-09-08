using System;
using System.Collections.Generic;
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
    public static string Language => Localization.Preference;
    private static readonly HashSet<object> EditingViews = new();
    public static bool IsEditing => EditingViews.Count != 0;
    internal static void BeginEditing(object view) => EditingViews.Add(view);
    internal static void EndEditing(object view) => EditingViews.Remove(view);
    public static event Action? Changed;

    public static bool TryApply(VirtualKey toggle, VirtualKey hold, string language, out string error)
    {
        error = "";
        if (!Keys.Contains(toggle) || !Keys.Contains(hold)) { error = Localization.Get("Chooseasupportedkeyormousebutton"); return false; }
        if (toggle == hold) { error = Localization.Get("Choosedifferentbuttonsfortoggleclickingandholdtoclick"); return false; }
        if (!Localization.TrySetLanguage(language, out error)) return false;
        ToggleKey = toggle;
        HoldKey = hold;
        Changed?.Invoke();
        return true;
    }

    public static string KeyLabel(VirtualKey key) => key switch
    {
        VirtualKey.LeftButton => Localization.Get("Mouse1left"), VirtualKey.RightButton => Localization.Get("Mouse2right"),
        VirtualKey.MiddleButton => Localization.Get("Mouse3middle"), VirtualKey.XButton1 => Localization.Get("Mouse4back"), VirtualKey.XButton2 => Localization.Get("Mouse5forward"),
        VirtualKey.LeftShift => Localization.Get("LeftShift"), VirtualKey.RightShift => Localization.Get("RightShift"),
        VirtualKey.LeftControl => Localization.Get("LeftCtrl"), VirtualKey.RightControl => Localization.Get("RightCtrl"),
        VirtualKey.LeftMenu => Localization.Get("LeftAlt"), VirtualKey.RightMenu => Localization.Get("RightAlt"),
        VirtualKey.PageUp => Localization.Get("PageUp"), VirtualKey.PageDown => Localization.Get("PageDown"),
        VirtualKey.Space or VirtualKey.Insert or VirtualKey.Delete or VirtualKey.Home or VirtualKey.End => Localization.Get(key.ToString()),
        _ => key.ToString().Replace("Number", "", StringComparison.Ordinal)
    };
}
