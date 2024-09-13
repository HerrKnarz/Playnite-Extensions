using KNARZhelper.Enum;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseStringType : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => true;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;

        public abstract override string LabelSingular { get; }

        public abstract override FieldType Type { get; }
    }
}