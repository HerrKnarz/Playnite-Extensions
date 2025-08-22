using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCover : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameCoverImageTitle");
        public override FieldType Type => FieldType.Cover;
        internal override string GetValue(Game game) => game.CoverImage;
        internal override void SetValue(Game game, string value) => game.CoverImage = API.Instance.Database.AddFile(value, game.Id);
    }
}