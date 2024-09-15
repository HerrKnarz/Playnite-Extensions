using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeSortingName : BaseStringType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameSortingNameTitle");
        public override FieldType Type => FieldType.SortingName;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.SortingName = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.SortingName.Trim().Any();

        public override bool GameContainsValue(Game game, string value) => game.SortingName.RegExIsMatch(value);
    }
}