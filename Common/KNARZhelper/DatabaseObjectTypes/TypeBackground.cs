using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeBackground : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameBackgroundTitle");
        public override FieldType Type => FieldType.Background;

        public override bool FieldInGameIsEmpty(Game game) => !game.BackgroundImage.Any();
    }
}