using Playnite.SDK;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeTimePlayed : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => false;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => false;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCTimePlayed");
        public override FieldType Type => FieldType.TimePlayed;
        public override ItemValueType ValueType => ItemValueType.Integer;

        public override bool AddValueToGame<T>(Game game, T value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.Playtime = (ulong)((value as int?) ?? 0);

                return true;
            });

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.Playtime > (ulong)((value as int?) ?? 0);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.Playtime < (ulong)((value as int?) ?? 0);
    }
}