using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseMediaType : IMetadataFieldType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public bool CanBeClearedInGame => false;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => false;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Media;

        public void EmptyFieldInGame(Game game)
        {
        }

        public abstract bool FieldInGameIsEmpty(Game game);
    }
}