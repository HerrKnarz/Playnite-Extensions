using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    public class TypeHdr : BaseBooleanType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameHdrTitle");
        public override FieldType Type => FieldType.Hdr;

        public override bool AddValueToGame(Game game, bool? value) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            game.EnableSystemHdr = value ?? false);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.EnableSystemHdr = false);

        public override bool GetValue(Game game) => game.EnableSystemHdr;
    }
}