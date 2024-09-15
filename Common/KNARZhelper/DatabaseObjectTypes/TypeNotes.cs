using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    internal class TypeNotes : BaseStringType
    {
        public override bool CanBeSetByMetadataAddOn => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCNotesLabel");
        public override FieldType Type => FieldType.Notes;

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.Notes = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.Notes.Trim().Any();
    }
}