using Playnite.SDK;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeDescription : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => true;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCGameDescriptionTitle");
        public override FieldType Type => FieldType.Description;
        public override ItemValueType ValueType => ItemValueType.String;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Description = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Description.Trim().Any();
    }
}