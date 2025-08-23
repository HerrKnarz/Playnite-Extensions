using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCriticScore : BaseIntegerType
    {
        public override string LabelSingular => ResourceProvider.GetString("LOCCriticScore");
        public override FieldType Type => FieldType.CriticScore;

        public override bool AddValueToGame(Game game, int? value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => (game.CriticScore = value) != null);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.CriticScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.CriticScore.HasValue;

        public override int? GetValue(Game game) => game.CriticScore;
    }
}