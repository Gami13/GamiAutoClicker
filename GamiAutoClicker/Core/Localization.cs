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

	public static readonly string[] Languages = ["system", "en", "pl"];
	public static string Preference { get; private set; } = LoadPreference();
	public static CultureInfo Culture { get; private set; } = ResolveCulture(Preference);
	public static event Action? Changed;

	public static string Get(string key) => new ResourceLoader().GetString(key);

	// Must precede App.InitializeComponent so XAML and code use the same language.
	public static void Initialize() => ApplicationLanguages.PrimaryLanguageOverride = Culture.Name;

	public static string Format(string key, params object[] arguments) => string.Format(Culture, Get(key), arguments);

	public static bool TrySetLanguage(string language, out string error) {
		error = string.Empty;
		if (!Languages.Contains(language)) { error = Get("UnsupportedLanguage"); return false; }
		if (Preference == language) return true;
		try {
			Directory.CreateDirectory(Path.GetDirectoryName(PreferencePath)!);
			File.WriteAllText(PreferencePath, language);
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException) {
			error = Get("LanguageSaveFailed");
			return false;
		}
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
			if (Languages.Contains(culture.TwoLetterISOLanguageName)) return culture;
		}
		return CultureInfo.GetCultureInfo("en");
	}
}
