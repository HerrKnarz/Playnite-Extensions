using KNARZhelper.MetadataCommon.Enum;
using MetadataUtilities.Enums;
using Playnite.SDK;

namespace MetadataUtilities.ViewModels
{
    public class FieldTypeContextItem
    {
        public ActionType ActionType { get; set; } = ActionType.None;
        public RelayCommand<FieldTypeContextItem> Command { get; set; }
        public ComparatorType Comparator { get; set; } = ComparatorType.None;
        public ConditionPropertyType ConditionPropertyType { get; set; } = ConditionPropertyType.Value;
        public FieldType FieldType { get; set; }
        public string Icon => ActionType != ActionType.None ? ActionType.GetEnumDisplayIcon() : Comparator != ComparatorType.None ? Comparator.GetEnumDisplayIcon() : ConditionPropertyType != ConditionPropertyType.Value ? ConditionPropertyType.GetEnumDisplayIcon() : string.Empty;
        public bool IsSeparator { get; set; } = false;
        public string Name { get; set; }
    }
}
