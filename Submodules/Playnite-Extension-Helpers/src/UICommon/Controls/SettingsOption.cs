using System.Windows;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace PlayniteExtensionHelpers.UICommon.Controls;

public class SettingsOption : SettingsBaseControl
{
    public static readonly DependencyProperty HorizontalControlAlignmentProperty =
        DependencyProperty.Register(
            "HorizontalControlAlignment",
            typeof(HorizontalAlignment),
            typeof(SettingsOption),
            new UIPropertyMetadata(HorizontalAlignment.Right));

    public static readonly DependencyProperty MinLabelWidthProperty =
        DependencyProperty.Register(
            "MinLabelWidth",
            typeof(double),
            typeof(SettingsOption),
            new UIPropertyMetadata(10.0));

    public HorizontalAlignment HorizontalControlAlignment
    {
        get => (HorizontalAlignment)GetValue(HorizontalControlAlignmentProperty);
        set => SetValue(HorizontalControlAlignmentProperty, value);
    }

    public double MinLabelWidth
    {
        get => (double)GetValue(MinLabelWidthProperty);
        set => SetValue(MinLabelWidthProperty, value);
    }
}