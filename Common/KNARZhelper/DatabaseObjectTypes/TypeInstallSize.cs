using Playnite.SDK;
using KNARZhelper.Enum;
using Playnite.SDK.Models;

// ReSharper disable ConditionIsAlwaysTrueOrFalse

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeInstallSize : BaseType
    {
        public override bool CanBeAdded => false;
        public override bool CanBeClearedInGame => true;
        public override bool CanBeDeleted => false;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeModified => false;
        public override bool CanBeSetInGame => true;
        public override bool IsList => false;
        public override string LabelSingular => ResourceProvider.GetString("LOCInstallSizeLabel");
        public override FieldType Type => FieldType.InstallSize;
        public override ItemValueType ValueType => ItemValueType.Integer;

        public override bool AddValueToGame<T>(Game game, T value) =>
            API.Instance.MainView.UIDispatcher.Invoke(() =>
            {
                game.InstallSize = (ulong)((value as int?) ?? 0);

                return true;
            });

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => game.InstallSize = default);

        public override bool FieldInGameIsEmpty(Game game) => !game.InstallSize.HasValue;

        public override bool IsBiggerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.InstallSize > (ulong)((value as int?) ?? 0);

        public override bool IsSmallerThan<T>(Game game, T value) =>
            (value != null || value is int) && game.InstallSize < (ulong)((value as int?) ?? 0);
    }
}