using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeUserScore : BaseIntegerType
    {
        public override bool CanBeSetByMetadataAddOn => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCUserScore");
        public override FieldType Type => FieldType.UserScore;

        public override bool AddValueToGame(Game game, int? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => (game.UserScore = value) != null);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.UserScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.UserScore.HasValue;

        public override ulong? GetValue(Game game) => (ulong?)game.UserScore;
    }
}