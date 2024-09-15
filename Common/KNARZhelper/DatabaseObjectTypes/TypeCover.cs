using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCover : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameCoverImageTitle");
        public override FieldType Type => FieldType.Cover;

        public override bool FieldInGameIsEmpty(Game game) => !game.CoverImage.Any();
    }
}