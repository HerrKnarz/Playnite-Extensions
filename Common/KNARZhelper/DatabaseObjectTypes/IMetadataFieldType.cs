using KNARZhelper.Enum;

namespace KNARZhelper.DatabaseObjectTypes
{
    public interface IMetadataFieldType
    {
        bool CanBeAdded { get; }
        bool CanBeClearedInGame { get; }
        bool CanBeDeleted { get; }
        bool CanBeEmptyInGame { get; }
        bool CanBeModified { get; }
        bool CanBeSetByMetadataAddOn { get; }
        bool CanBeSetInGame { get; }
        string LabelPlural { get; }
        string LabelSingular { get; }
        FieldType Type { get; }
        ItemValueType ValueType { get; }
    }
}