using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeIcon : BaseMediaType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCGameIconTitle");
        public override FieldType Type => FieldType.Icon;

        public override bool FieldInGameIsEmpty(Game game) => !game.Icon.Any();
    }
}