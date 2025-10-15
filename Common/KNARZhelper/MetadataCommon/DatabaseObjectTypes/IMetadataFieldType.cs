using KNARZhelper.MetadataCommon.Enum;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Represents the metadata field type, defining its characteristics and constraints regarding how it can be used,
    /// modified, and represented within the system.
    /// </summary>
    /// <remarks>This interface provides properties to determine the capabilities and limitations of a
    /// metadata field, such as whether it can be added, modified, or cleared in specific contexts. It also includes
    /// properties for retrieving the field's type, value type, and singular/plural labels for display
    /// purposes.</remarks>
    public interface IMetadataFieldType
    {
        /// <summary>
        /// Defines whether the metadata field can be added to the database.
        /// </summary>
        bool CanBeAdded { get; }

        /// <summary>
        /// Defines whether the metadata field can be cleared in a game.
        /// </summary>
        bool CanBeClearedInGame { get; }

        /// <summary>
        /// Defines whether the metadata field can be deleted from the database.
        /// </summary>
        bool CanBeDeleted { get; }

        /// <summary>
        /// Defines whether the metadata field can be empty in a game.
        /// </summary>
        bool CanBeEmptyInGame { get; }

        /// <summary>
        /// Defines whether the value(s) in the metadata field can be modified.
        /// </summary>
        bool CanBeModified { get; }

        /// <summary>
        /// Defines whether the metadata field can be set by the metadata add-on.
        /// </summary>
        bool CanBeSetByMetadataAddOn { get; }

        /// <summary>
        /// Defines whether the metadata field can be set in a game.
        /// </summary>
        bool CanBeSetInGame { get; }

        /// <summary>
        /// Label for the plural form of the metadata field, used for display purposes.
        /// </summary>
        string LabelPlural { get; }

        /// <summary>
        /// Label for the singular form of the metadata field, used for display purposes.
        /// </summary>
        string LabelSingular { get; }

        /// <summary>
        /// Type of the metadata field, indicating its nature and how it should be handled.
        /// </summary>
        FieldType Type { get; }

        /// <summary>
        /// Type of value represented by the item.
        /// </summary>
        ItemValueType ValueType { get; }
    }
}