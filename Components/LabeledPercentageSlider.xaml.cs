using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;

namespace GamiAutoClicker.Components;

public sealed partial class LabeledPercentageSlider : UserControl {
	public static readonly DependencyProperty HeaderProperty =
		DependencyProperty.Register(
			nameof(Header),
			typeof(string),
			typeof(LabeledPercentageSlider),
			new PropertyMetadata(string.Empty, OnHeaderChanged));

	public static readonly DependencyProperty PercentageProperty =
		DependencyProperty.Register(
			nameof(Percentage),
			typeof(double),
			typeof(LabeledPercentageSlider),
			new PropertyMetadata(0d, OnPercentageChanged));

	public string Header {
		get => (string)GetValue(HeaderProperty);
		set => SetValue(HeaderProperty, value);
	}

	public double Percentage {
		get => (double)GetValue(PercentageProperty);
		set => SetValue(PercentageProperty, value);
	}

	public event RoutedEventHandler? PercentageChanged;

	public LabeledPercentageSlider() {
		InitializeComponent();
		UpdateVisualState();
	}

	private static void OnHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is LabeledPercentageSlider control) {
			control.UpdateVisualState();
		}
	}

	private static void OnPercentageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
		if (d is not LabeledPercentageSlider control) return;

		double value = Math.Clamp((double)e.NewValue, 0d, 1d);
		if (value != (double)e.NewValue) {
			control.Percentage = value;
			return;
		}

		control.UpdateVisualState();
	}

	private void OnSliderValueChanged(object sender, RangeBaseValueChangedEventArgs e) {
		Percentage = e.NewValue;
		PercentageChanged?.Invoke(this, new RoutedEventArgs());
	}

	private void UpdateVisualState() {
		if (HeaderText != null) {
			HeaderText.Text = Header;
			HeaderText.Visibility = string.IsNullOrEmpty(Header)
				? Visibility.Collapsed
				: Visibility.Visible;
		}

		if (ValueText != null) {
			ValueText.Text = FormatPercentage(Percentage);
		}

		if (ValueSlider != null && ValueSlider.Value != Percentage) {
			ValueSlider.Value = Percentage;
		}

		if (ValueSlider != null) {
			AutomationProperties.SetName(ValueSlider, $"{Header}: {FormatPercentage(Percentage)}");
		}
	}

	private static string FormatPercentage(double value) => $"{Math.Round(value * 100):0}%";
}
