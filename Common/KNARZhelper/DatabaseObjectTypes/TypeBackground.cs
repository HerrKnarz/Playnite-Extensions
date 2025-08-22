using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeBackground : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameBackgroundTitle");
        public override FieldType Type => FieldType.Background;
        internal override string GetValue(Game game) => game.BackgroundImage;
        internal override void SetValue(Game game, string value) => game.BackgroundImage = API.Instance.Database.AddFile(value, game.Id);
    }
}