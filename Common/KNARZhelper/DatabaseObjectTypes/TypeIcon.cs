using Playnite.SDK;
using System;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeIcon : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => false;
        public override bool IsList => false;
        public override string Label => ResourceProvider.GetString("LOCGameIconTitle");
        public override FieldType Type => FieldType.Icon;
        public override ItemValueType ValueType => ItemValueType.None;

        public override bool DbObjectExists(string name) => false;

        public override bool DbObjectInGame(Game game, Guid id) => false;

        public override bool DbObjectInUse(Guid id) => false;

        public override void EmptyFieldInGame(Game game)
        {
        }

        public override bool FieldInGameIsEmpty(Game game) => !game.Icon.Any();

        public override Guid GetDbObjectId(string name) => default;

        public override int GetGameCount(Guid id, bool ignoreHidden = false) => 0;

        public override bool NameExists(string name, Guid id) => false;
    }
}