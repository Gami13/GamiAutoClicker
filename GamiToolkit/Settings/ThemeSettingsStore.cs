using Gami;
using Microsoft.UI.Composition.SystemBackdrops;
using ProtoBuf;
using System;
using System.IO;
using Windows.UI;

namespace GamiToolkit.Settings;

public static class GamiConfiguration
{
    public static string RootDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "config", "GamiToolkit");

    public static string GetFilePath(string name) {
        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || Path.IsPathRooted(name) || name.Contains("..", StringComparison.Ordinal))
            throw new ArgumentException("The configuration name must be a file name.", nameof(name));
        return Path.Combine(RootDirectory, name);
    }
}

[ProtoContract]
public sealed class ThemeSettingsData
{
    [ProtoMember(1)] public int BackdropMaterial { get; set; }
    [ProtoMember(2)] public int Theme { get; set; }
    [ProtoMember(3)] public bool ShouldOverride { get; set; }
    [ProtoMember(4)] public uint FallbackColor { get; set; }
    [ProtoMember(5)] public uint TintColor { get; set; }
    [ProtoMember(6)] public float TintOpacity { get; set; }
    [ProtoMember(7)] public float LuminosityOpacity { get; set; }
}

public static class ThemeSettingsStore
{
    public static string FilePath => GamiConfiguration.GetFilePath(Path.Combine("theme", "settings.bin"));

    public static void Load() {
        if (!File.Exists(FilePath)) return;
        try {
            using var stream = File.OpenRead(FilePath);
            Apply(Serializer.Deserialize<ThemeSettingsData>(stream));
        } catch (Exception) { /* A corrupt optional settings file should not prevent app startup. */ }
    }

    public static void Save() {
        var data = FromCurrentTheme();
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temporary = FilePath + ".tmp";
        using (var stream = File.Create(temporary)) Serializer.Serialize(stream, data);
        File.Move(temporary, FilePath, true);
    }

    public static void Apply(ThemeSettingsData data) {
        if (Enum.IsDefined((EasyWindows.BackdropMaterial)data.BackdropMaterial)) EasyWindows.SetBackdropMaterial((EasyWindows.BackdropMaterial)data.BackdropMaterial);
        if (Enum.IsDefined((SystemBackdropTheme)data.Theme)) EasyWindows.ApplyTheme((SystemBackdropTheme)data.Theme);
        EasyWindows.SetOverrides(data.ShouldOverride);
        EasyWindows.SetFallbackColor(ToColor(data.FallbackColor));
        EasyWindows.SetTintColor(ToColor(data.TintColor));
        if (data.TintOpacity is >= 0 and <= 1) EasyWindows.SetTintOpacity(data.TintOpacity);
        if (data.LuminosityOpacity is >= 0 and <= 1) EasyWindows.SetLuminosityOpacity(data.LuminosityOpacity);
    }

    private static ThemeSettingsData FromCurrentTheme() => new() {
        BackdropMaterial = (int)EasyWindows.Theme.BackdropMaterial, Theme = (int)EasyWindows.Theme.Theme,
        ShouldOverride = EasyWindows.Theme.ShouldOverride, FallbackColor = ToUInt(EasyWindows.Theme.FallbackColor),
        TintColor = ToUInt(EasyWindows.Theme.TintColor), TintOpacity = EasyWindows.Theme.TintOpacity,
        LuminosityOpacity = EasyWindows.Theme.LuminosityOpacity
    };

    private static uint ToUInt(Color c) => (uint)(c.A << 24 | c.R << 16 | c.G << 8 | c.B);
    private static Color ToColor(uint value) => new() { A = (byte)(value >> 24), R = (byte)(value >> 16), G = (byte)(value >> 8), B = (byte)value };
}

public static class CustomSettingsStore
{
    public static string GetFilePath(string name) => GamiConfiguration.GetFilePath(name + ".bin");
    public static void Save<T>(string name, T settings) {
        var path = GetFilePath(name); Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var stream = File.Create(path); Serializer.Serialize(stream, settings);
    }
    public static T? Load<T>(string name) {
        var path = GetFilePath(name); if (!File.Exists(path)) return default;
        using var stream = File.OpenRead(path); return Serializer.Deserialize<T>(stream);
    }
}
