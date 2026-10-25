using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using Windows.System.UserProfile;

namespace GamiAutoClicker;

// The unpackaged Reactor host reads the same .resw files as the XAML app.
// WinAppSDK's ResourceLoader requires a resource context that this host does not provide.
internal static class Localization
{
    private const string ResourcePrefix = "GamiAutoClicker.Reactor.Strings.";
    private const string ResourceSuffix = ".Resources.resw";
    private static readonly Assembly ResourceAssembly = typeof(Localization).Assembly;
    private static readonly string PreferencePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GamiAutoClicker", "language.txt");
    public static readonly string[] Languages =
    [
        "system",
        .. ResourceAssembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal) &&
                           name.EndsWith(ResourceSuffix, StringComparison.Ordinal))
            .Select(name => name[ResourcePrefix.Length..^ResourceSuffix.Length])
            .Order(StringComparer.Ordinal)
    ];
    private static readonly Dictionary<string, Lazy<Dictionary<string, string>>> Resources = Languages.Skip(1)
        .ToDictionary(tag => tag, tag => new Lazy<Dictionary<string, string>>(() => Load(tag)), StringComparer.OrdinalIgnoreCase);
    public static string Preference { get; private set; } = LoadPreference();
    public static CultureInfo Culture { get; private set; } = ResolveCulture(Preference);
    public static event Action? Changed;

    public static string Get(string key)
    {
        string? language = Resources.Keys.FirstOrDefault(tag => Culture.Name.Equals(tag, StringComparison.OrdinalIgnoreCase))
            ?? Resources.Keys.FirstOrDefault(tag => Culture.Name.StartsWith(tag + "-", StringComparison.OrdinalIgnoreCase));
        if (language is not null && Resources[language].Value.TryGetValue(key, out string? translated))
            return translated;
        return Resources["en"].Value.TryGetValue(key, out string? fallback) ? fallback : key;
    }

    public static string Format(string key, params object[] arguments) =>
        string.Format(Culture, Get(key), arguments);

    public static string LanguageLabel(string language)
    {
        if (language == "system") return Get("FollowWindows");
        CultureInfo culture = CultureInfo.GetCultureInfo(language);
        string name = culture.NativeName;
        if (!culture.IsNeutralCulture)
        {
            string region = new RegionInfo(culture.Name).TwoLetterISORegionName;
            name = $"{culture.Parent.NativeName} ({region})";
        }
        return culture.TextInfo.ToUpper(name[..1]) + name[1..];
    }

    public static bool TrySetLanguage(string language, out string error)
    {
        error = string.Empty;
        if (!Languages.Contains(language))
        {
            error = Get("UnsupportedLanguage");
            return false;
        }
        if (Preference == language) return true;
        Preference = language;
        Culture = ResolveCulture(language);
        Changed?.Invoke();
        return true;
    }

    private static string LoadPreference()
    {
        try
        {
            string value = File.ReadAllText(PreferencePath).Trim();
            return Languages.Contains(value) ? value : "system";
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return "system";
        }
    }

    private static CultureInfo ResolveCulture(string language)
    {
        if (language != "system") return CultureInfo.GetCultureInfo(language);
        foreach (string preferred in GlobalizationPreferences.Languages)
        {
            CultureInfo culture = CultureInfo.GetCultureInfo(preferred);
            for (CultureInfo candidate = culture; candidate.Name.Length > 0; candidate = candidate.Parent)
                if (Languages.Contains(candidate.Name, StringComparer.OrdinalIgnoreCase)) return culture;
        }
        return CultureInfo.GetCultureInfo("en");
    }

    private static Dictionary<string, string> Load(string language)
    {
        using Stream stream = ResourceAssembly.GetManifestResourceStream(ResourcePrefix + language + ResourceSuffix)
            ?? throw new InvalidOperationException($"Missing translation: {language}");
        return XDocument.Load(stream).Root!.Elements("data")
            .Where(element => element.Attribute("name") is not null)
            .ToDictionary(element => (string)element.Attribute("name")!,
                element => (string?)element.Element("value") ?? string.Empty, StringComparer.Ordinal);
    }
}
