using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeHidden : BaseBooleanType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameHiddenTitle");
        public override FieldType Type => FieldType.Hidden;

        public override bool AddValueToGame(Game game, bool? value) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.Hidden = value ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Hidden = false);

        public override bool GetValue(Game game) => game.Hidden;
    }
}