using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Markup;
namespace GamiToolkit.Components;

// Resource expressions use WinUI's native theme resolution, including high contrast.
internal static class ResourceStyles {
	internal static ResourceDictionary NavigationPaneResources() => (ResourceDictionary)XamlReader.Load(
		"""
        <ResourceDictionary xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
            <ResourceDictionary.ThemeDictionaries>
                <ResourceDictionary x:Key="Default">
                    <SolidColorBrush x:Key="NavigationViewDefaultPaneBackground" Color="Transparent" />
                    <SolidColorBrush x:Key="NavigationViewExpandedPaneBackground" Color="Transparent" />
                </ResourceDictionary>
                <ResourceDictionary x:Key="Light">
                    <SolidColorBrush x:Key="NavigationViewDefaultPaneBackground" Color="Transparent" />
                    <SolidColorBrush x:Key="NavigationViewExpandedPaneBackground" Color="Transparent" />
                </ResourceDictionary>
                <ResourceDictionary x:Key="HighContrast">
                    <SolidColorBrush x:Key="NavigationViewDefaultPaneBackground" Color="{ThemeResource SystemColorWindowColor}" />
                    <SolidColorBrush x:Key="NavigationViewExpandedPaneBackground" Color="{ThemeResource SystemColorWindowColor}" />
                </ResourceDictionary>
            </ResourceDictionary.ThemeDictionaries>
        </ResourceDictionary>
        """);
	internal static Style ColorFlyoutPresenterStyle() => (Style)XamlReader.Load(
		"""
        <Style xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" TargetType="FlyoutPresenter">
            <Setter Property="Background" Value="Transparent" />
            <Setter Property="BorderBrush" Value="{ThemeResource SurfaceStrokeColorFlyoutBrush}" />
            <Setter Property="BorderThickness" Value="1" />
            <Setter Property="CornerRadius" Value="{ThemeResource OverlayCornerRadius}" />
            <Setter Property="Padding" Value="12" />
        </Style>
        """);
}
