using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Windows.ApplicationModel.Resources;
using Microsoft.Windows.Globalization;
using Windows.System.UserProfile;

namespace GamiAutoClicker;

internal static class Localization {
	private static readonly string PreferencePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GamiAutoClicker", "language.txt");

	public static readonly string[] Languages = ["system", .. SupportedLanguages.Tags];
	public static string Preference { get; private set; } = LoadPreference();
	public static CultureInfo Culture { get; private set; } = ResolveCulture(Preference);
	public static event Action? Changed;

	public static string Get(string key) => new ResourceLoader().GetString(key);

	// Must precede App.InitializeComponent so XAML and code use the same language.
	public static void Initialize() => ApplicationLanguages.PrimaryLanguageOverride = Culture.Name;

	public static string Format(string key, params object[] arguments) => string.Format(Culture, Get(key), arguments);

	public static string LanguageLabel(string language) {
		if (language == "system") return Get("FollowWindows");
		var culture = CultureInfo.GetCultureInfo(language);
		string name = culture.NativeName;
		if (!culture.IsNeutralCulture) {
			// Keep script names (e.g. Chinese Traditional), but abbreviate the region.
			string region = new RegionInfo(culture.Name).TwoLetterISORegionName;
			name = $"{culture.Parent.NativeName} ({region})";
		}
		return culture.TextInfo.ToUpper(name[..1]) + name[1..];
	}

	public static bool TrySetLanguage(string language, out string error) {
		error = string.Empty;
		if (!Languages.Contains(language)) { error = Get("UnsupportedLanguage"); return false; }
		if (Preference == language) return true;
		Preference = language;
		Culture = ResolveCulture(language);
		ApplicationLanguages.PrimaryLanguageOverride = Culture.Name;
		Changed?.Invoke();
		return true;
	}

	private static string LoadPreference() {
		try {
			string value = File.ReadAllText(PreferencePath).Trim();
			return Languages.Contains(value) ? value : "system";
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) { return "system"; }
	}

	private static CultureInfo ResolveCulture(string language) {
		if (language != "system") return CultureInfo.GetCultureInfo(language);
		// Use the Windows display-language list, independent of regional number formatting.
		foreach (string preferred in GlobalizationPreferences.Languages) {
			var culture = CultureInfo.GetCultureInfo(preferred);
			for (CultureInfo candidate = culture; candidate.Name.Length > 0; candidate = candidate.Parent) {
				if (Languages.Contains(candidate.Name, StringComparer.OrdinalIgnoreCase)) return culture;
			}
			// A neutral Windows preference can also select an implemented regional variant.
			string? variant = Languages.Skip(1).FirstOrDefault(name => name.StartsWith(culture.Name + "-", StringComparison.OrdinalIgnoreCase));
			if (variant is not null) return CultureInfo.GetCultureInfo(variant);
		}
		return CultureInfo.GetCultureInfo("en");
	}
}
