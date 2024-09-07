using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeBackground : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCGameBackgroundTitle");
        public override FieldType Type => FieldType.Background;
        public override ItemValueType ValueType => ItemValueType.Media;

        public override bool FieldInGameIsEmpty(Game game) => !game.BackgroundImage.Any();
    }
}