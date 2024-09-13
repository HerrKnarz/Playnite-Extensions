using Playnite.SDK;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCover : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameCoverImageTitle");
        public override FieldType Type => FieldType.Cover;

        public override bool FieldInGameIsEmpty(Game game) => !game.CoverImage.Any();
    }
}