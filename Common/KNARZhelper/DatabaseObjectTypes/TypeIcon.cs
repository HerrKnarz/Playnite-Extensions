using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeIcon : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameIconTitle");
        public override FieldType Type => FieldType.Icon;
        internal override string GetValue(Game game) => game.Icon;
        internal override void SetValue(Game game, string value) => game.Icon = API.Instance.Database.AddFile(value, game.Id);
    }
}