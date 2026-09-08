using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace GamiAutoClicker.Components;

internal sealed partial class RestoreDefaultsButton : UserControl
{
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(RestoreDefaultsButton),
        new PropertyMetadata(true, OnIsExpandedChanged));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public event RoutedEventHandler Click
    {
        add => RestoreButton.Click += value;
        remove => RestoreButton.Click -= value;
    }

    public RestoreDefaultsButton()
    {
        InitializeComponent();
        UpdateVisualState();
    }

    private static void OnIsExpandedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        ((RestoreDefaultsButton)sender).UpdateVisualState();
    }

    private void UpdateVisualState()
    {
        if (RestoreButton is null) return;
        bool compact = !IsExpanded;
        RestoreText.Visibility = compact ? Visibility.Collapsed : Visibility.Visible;
        RestoreIcon.FontSize = compact ? 16 : 14;
        RestoreButton.Width = compact ? 38 : double.NaN;
        RestoreButton.Height = compact ? 38 : double.NaN;
        RestoreButton.Padding = compact ? new Thickness(0) : new Thickness(11, 5, 11, 6);
    }
}
