using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.System;

namespace GamiAutoClicker.Pages.Settings;

internal sealed partial class GeneralPage : Page {
	private bool _isSynchronizing;

	public GeneralPage() {
		InitializeComponent();
		_isSynchronizing = true;
		RefreshControls();
	}

	private void OnLoaded(object sender, RoutedEventArgs e) {
        RefreshControls();
        UpdateLayout(RootGrid.ActualWidth);
    }
	private void OnSizeChanged(object sender, SizeChangedEventArgs e) => UpdateLayout(e.NewSize.Width);

	private void UpdateLayout(double width) {
		RestoreDefaultsControl.IsExpanded = width >= RestoreDefaultsBreakpoint;
		bool wide = width >= 590;
		RightColumn.Width = new GridLength(wide ? 1 : 0, GridUnitType.Star);
		Grid.SetColumn(LanguageCard, wide ? 1 : 0);
		Grid.SetRow(LanguageCard, wide ? 0 : 1);
	}

	private void RefreshControls() {
		_isSynchronizing = true;
		ToggleKeyComboBox.ItemsSource = GeneralSettings.Keys.Select(GeneralSettings.KeyLabel).ToArray();
		HoldKeyComboBox.ItemsSource = GeneralSettings.Keys.Select(GeneralSettings.KeyLabel).ToArray();
		LanguageComboBox.ItemsSource = Localization.Languages.Select(language => new ComboBoxItem {
			Content = Localization.LanguageLabel(language),
			Tag = language
		}).ToArray();
		ToggleKeyComboBox.SelectedIndex = System.Array.IndexOf(GeneralSettings.Keys, GeneralSettings.ToggleKey);
		HoldKeyComboBox.SelectedIndex = System.Array.IndexOf(GeneralSettings.Keys, GeneralSettings.HoldKey);
		LanguageComboBox.SelectedItem = LanguageComboBox.Items.OfType<ComboBoxItem>()
			.FirstOrDefault(item => item.Tag is string language && language == GeneralSettings.Language);
		_isSynchronizing = false;
	}

	private void OnSettingChanged(object sender, SelectionChangedEventArgs e) {
		if (_isSynchronizing || ToggleKeyComboBox.SelectedIndex < 0 || HoldKeyComboBox.SelectedIndex < 0
			|| LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string language }) {
			return;
		}

		Apply(GeneralSettings.Keys[ToggleKeyComboBox.SelectedIndex], GeneralSettings.Keys[HoldKeyComboBox.SelectedIndex], language);
	}

	private void Apply(VirtualKey toggle, VirtualKey hold, string language) {
		ErrorInfo.IsOpen = !GeneralSettings.TryApply(toggle, hold, language, out string error);
		ErrorInfo.Message = error;
		RefreshControls();
	}

	private void OnRestoreDefaultsClick(object sender, RoutedEventArgs e) => Apply(VirtualKey.F8, VirtualKey.XButton1, "system");

	private const double RestoreDefaultsBreakpoint = 470;
}
