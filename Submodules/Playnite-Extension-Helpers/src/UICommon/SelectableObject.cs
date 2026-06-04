using CommunityToolkit.Mvvm.ComponentModel;

namespace PlayniteExtensionHelpers.UICommon;

public partial class SelectableObject<T> : ObservableObject
{
    public SelectableObject(T objectData)
    {
        ObjectData = objectData;
    }

    public SelectableObject(T objectData, bool isSelected)
    {
        IsSelected = isSelected;
        ObjectData = objectData;
    }

    [ObservableProperty]
    public partial bool IsSelected { get; set; } = false;

    [ObservableProperty]
    public partial T ObjectData { get; set; }
}