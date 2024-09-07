using Playnite.SDK;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeCriticScore : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCCriticScore");
        public override FieldType Type => FieldType.CriticScore;
        public override ItemValueType ValueType => ItemValueType.Integer;

        public override bool AddValueToGame<T>(Game game, T value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => (game.CriticScore = (value as int?) ?? default) != null);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.CriticScore = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.CriticScore.HasValue;

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.CriticScore > (value as int?);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.CriticScore < (value as int?);
    }
}