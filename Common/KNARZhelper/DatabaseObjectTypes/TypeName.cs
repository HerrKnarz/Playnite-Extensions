using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeName : BaseStringType
    {
        public override bool CanBeClearedInGame => false;

        public override string LabelSingular => ResourceProvider.GetString("LOCGameNameTitle");
        public override FieldType Type => FieldType.Name;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Name = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Name.Trim().Any();

        public override bool GameContainsValue(Game game, string value) => game.Name.RegExIsMatch(value);
    }
}