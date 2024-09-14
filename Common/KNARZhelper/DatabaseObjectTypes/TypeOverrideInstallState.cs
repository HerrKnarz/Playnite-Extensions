using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeOverrideInstallState : BaseBooleanType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCOverrideInstallState");
        public override FieldType Type => FieldType.OverrideInstallState;

        public override bool AddValueToGame(Game game, bool? value) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.OverrideInstallState = value ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.OverrideInstallState = false);

        public override bool GetValue(Game game) => game.OverrideInstallState;
    }
}