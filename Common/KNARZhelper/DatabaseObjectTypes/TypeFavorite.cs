using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeFavorite : BaseBooleanType
    {
        public override string LabelPlural => ResourceProvider.GetString("LOCQuickFilterFavorites");
        public override string LabelSingular => ResourceProvider.GetString("LOCGameFavoriteTitle");
        public override FieldType Type => FieldType.Favorite;

        public override bool AddValueToGame(Game game, bool? value) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.Favorite = value ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Favorite = false);

        public override bool GetValue(Game game) => game.Favorite;
    }
}