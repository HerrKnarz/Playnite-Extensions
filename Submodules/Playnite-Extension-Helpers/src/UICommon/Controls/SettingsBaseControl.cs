using System.Windows;
using System.Windows.Controls;

namespace PlayniteExtensionHelpers.UICommon.Controls;

public class SettingsBaseControl : ContentControl
{
    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            "Description",
            typeof(string),
            typeof(SettingsBaseControl),
            new UIPropertyMetadata(string.Empty));

    public static readonly DependencyProperty HeadingProperty =
        DependencyProperty.Register(
            "Heading",
            typeof(string),
            typeof(SettingsBaseControl),
            new UIPropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Heading
    {
        get => (string)GetValue(HeadingProperty);
        set => SetValue(HeadingProperty, value);
    }
}