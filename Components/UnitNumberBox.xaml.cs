using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Diagnostics.CodeAnalysis;
using Windows.Foundation;
using Windows.Globalization.NumberFormatting;

namespace GamiAutoClicker.Components;

[SuppressMessage("Design", "CA1515:Consider making public types internal", Justification = "The control is referenced from XAML.")]
public sealed partial class UnitNumberBox : UserControl
{
	private readonly DecimalFormatter _numberFormatter = new()
	{
		IntegerDigits = 1,
		FractionDigits = 0
	};
	private bool _isUpdatingDisplay;

	public static readonly DependencyProperty ValueProperty =
		DependencyProperty.Register(
			nameof(Value),
			typeof(double),
			typeof(UnitNumberBox),
			new PropertyMetadata(0d, OnDisplayPropertyChanged));

	public static readonly DependencyProperty MinimumProperty =
		DependencyProperty.Register(
			nameof(Minimum),
			typeof(double),
			typeof(UnitNumberBox),
			new PropertyMetadata(double.MinValue, OnDisplayPropertyChanged));

	public static readonly DependencyProperty MaximumProperty =
		DependencyProperty.Register(
			nameof(Maximum),
			typeof(double),
			typeof(UnitNumberBox),
			new PropertyMetadata(double.MaxValue, OnDisplayPropertyChanged));

	public static readonly DependencyProperty SmallChangeProperty =
		DependencyProperty.Register(
			nameof(SmallChange),
			typeof(double),
			typeof(UnitNumberBox),
			new PropertyMetadata(1d, OnDisplayPropertyChanged));

	public static readonly DependencyProperty UnitProperty =
		DependencyProperty.Register(
			nameof(Unit),
			typeof(string),
			typeof(UnitNumberBox),
			new PropertyMetadata(string.Empty, OnDisplayPropertyChanged));

	public static readonly DependencyProperty DecimalPlacesProperty =
		DependencyProperty.Register(
			nameof(DecimalPlaces),
			typeof(int),
			typeof(UnitNumberBox),
			new PropertyMetadata(0, OnDisplayPropertyChanged));

	[SuppressMessage("Naming", "CA1721:Property names should not match get methods", Justification = "Value is the conventional name for a WinUI dependency property.")]
	public double Value
	{
		get => (double)GetValue(ValueProperty);
		set => SetValue(ValueProperty, value);
	}

	public double Minimum
	{
		get => (double)GetValue(MinimumProperty);
		set => SetValue(MinimumProperty, value);
	}

	public double Maximum
	{
		get => (double)GetValue(MaximumProperty);
		set => SetValue(MaximumProperty, value);
	}

	public double SmallChange
	{
		get => (double)GetValue(SmallChangeProperty);
		set => SetValue(SmallChangeProperty, value);
	}

	public string Unit
	{
		get => (string)GetValue(UnitProperty);
		set => SetValue(UnitProperty, value);
	}

	public int DecimalPlaces
	{
		get => (int)GetValue(DecimalPlacesProperty);
		set => SetValue(DecimalPlacesProperty, value);
	}

	[SuppressMessage("Design", "CA1003:Use generic event handler instances", Justification = "The event deliberately forwards the native NumberBox event arguments.")]
	public event TypedEventHandler<UnitNumberBox, NumberBoxValueChangedEventArgs>? ValueChanged;

	public UnitNumberBox()
	{
		InitializeComponent();
		UpdateDisplay();
	}

	private static void OnDisplayPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is UnitNumberBox control && !control._isUpdatingDisplay)
		{
			control.UpdateDisplay(e.Property);
		}
	}

	private void OnInputNumberBoxValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
	{
		if (_isUpdatingDisplay) return;

		Value = args.NewValue;
		ValueChanged?.Invoke(this, args);
	}

	private void OnInputNumberBoxLoaded(object sender, RoutedEventArgs e)
	{
		RemoveClearButton(InputNumberBox);
	}

	private void UpdateDisplay(DependencyProperty? property = null)
	{
		if (InputNumberBox is null || UnitTextBlock is null) return;

		_isUpdatingDisplay = true;
		try
		{
			if (property is null || property == MinimumProperty)
				InputNumberBox.Minimum = Minimum;
			if (property is null || property == MaximumProperty)
				InputNumberBox.Maximum = Maximum;
			if (property is null || property == SmallChangeProperty)
				InputNumberBox.SmallChange = SmallChange;
			if (property is null || property == UnitProperty)
				UnitTextBlock.Text = Unit;

			if (property is null || property == DecimalPlacesProperty)
			{
				_numberFormatter.FractionDigits = Math.Clamp(DecimalPlaces, 0, 15);
				InputNumberBox.NumberFormatter = _numberFormatter;
			}

			if (property is null || property == MinimumProperty || property == MaximumProperty)
			{
				// Range changes can change the value; display rounding must not.
				Value = Math.Clamp(Value, InputNumberBox.Minimum, InputNumberBox.Maximum);
			}

			if (property is null || property == ValueProperty || property == DecimalPlacesProperty
				|| property == MinimumProperty || property == MaximumProperty)
			{
				InputNumberBox.Value = Math.Round(Value, Math.Clamp(DecimalPlaces, 0, 15), MidpointRounding.AwayFromZero);
			}
		}
		finally
		{
			_isUpdatingDisplay = false;
		}
	}

	private static void RemoveClearButton(DependencyObject root)
	{
		DependencyObject? clearButton = FindChildElementByName(root, "DeleteButton");
		if (clearButton is UIElement button && VisualTreeHelper.GetParent(clearButton) is Panel parent)
		{
			parent.Children.Remove(button);
		}
	}

	private static DependencyObject? FindChildElementByName(DependencyObject tree, string name)
	{
		for (int index = 0; index < VisualTreeHelper.GetChildrenCount(tree); index++)
		{
			DependencyObject child = VisualTreeHelper.GetChild(tree, index);
			if (child is FrameworkElement element && element.Name == name)
			{
				return child;
			}

			DependencyObject? childInSubtree = FindChildElementByName(child, name);
			if (childInSubtree is not null)
			{
				return childInSubtree;
			}
		}

		return null;
	}
}
