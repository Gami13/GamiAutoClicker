using Gami;

namespace GamiAutoClicker.Components.Settings;

internal static class AppearanceText {
	internal static string MaterialName(EasyWindows.BackdropMaterial material) => material switch {
		EasyWindows.BackdropMaterial.MicaAlt => Localization.Get("MicaAlt"),
		EasyWindows.BackdropMaterial.AcrylicThin => Localization.Get("ThinAcrylic"),
		_ => Localization.Get(material.ToString())
	};
}
