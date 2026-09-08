using System;
using System.IO;
using GamiToolkit.Settings;
using ProtoBuf;
using Windows.System;

namespace GamiAutoClicker;

[ProtoContract]
internal sealed class SettingsData {
	[ProtoMember(1)] public VirtualKey ToggleKey { get; set; } = VirtualKey.F8;
	[ProtoMember(2)] public VirtualKey HoldKey { get; set; } = VirtualKey.XButton1;
	[ProtoMember(3)] public string Language { get; set; } = "system";
	[ProtoMember(4)] public ThemeSettingsData? Appearance { get; set; }
}

// Live settings remain in GeneralSettings and EasyWindows. This snapshot changes
// only after loading or successfully saving the complete settings file.
internal static class SettingsStore {
	internal static string FilePath => Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GamiAutoClicker", "settings.bin");
	private static SettingsData? _persisted;

	internal static void Load() {
		// Retain preferences from older versions until the first explicit Save.
		ThemeSettingsStore.Load();
		if (File.Exists(FilePath)) {
			try {
				using FileStream stream = File.OpenRead(FilePath);
				Apply(Serializer.Deserialize<SettingsData>(stream));
			}
			catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ProtoException or ArgumentException) {
				// Missing or damaged optional preferences must not block startup.
			}
		}
		_persisted = Capture();
	}

	internal static bool TrySave(out string error) {
		SettingsData snapshot = Capture();
		string temporary = FilePath + ".tmp";
		try {
			Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
			using (FileStream stream = File.Create(temporary)) Serializer.Serialize(stream, snapshot);
			File.Move(temporary, FilePath, true);
			_persisted = snapshot;
			error = string.Empty;
			return true;
		}
		catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ProtoException) {
			error = Localization.Get("SettingsSaveFailed");
			return false;
		}
	}

	// Called for a real window close, never when localization replaces its content.
	internal static void DiscardChanges() {
		if (_persisted is not null) Apply(_persisted);
	}

	private static SettingsData Capture() => new() {
		ToggleKey = GeneralSettings.ToggleKey,
		HoldKey = GeneralSettings.HoldKey,
		Language = GeneralSettings.Language,
		Appearance = ThemeSettingsStore.FromCurrentTheme()
	};

	private static void Apply(SettingsData settings) {
		GeneralSettings.TryApply(settings.ToggleKey, settings.HoldKey, settings.Language, out _);
		if (settings.Appearance is not null) ThemeSettingsStore.Apply(settings.Appearance);
	}
}
