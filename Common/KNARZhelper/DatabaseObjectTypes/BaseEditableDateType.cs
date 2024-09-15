using Playnite.SDK.Models;
using System;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseEditableDateType : BaseDateType, IValueType, IClearAbleType
    {
        public override bool CanBeClearedInGame => true;
        public override bool CanBeEmptyInGame => true;
        public override bool CanBeSetByMetadataAddOn => true;
        public override bool CanBeSetInGame => true;

        public bool AddValueToGame<T>(Game game, T value) => AddValueToGame(game, value as DateTime?);

        public abstract bool AddValueToGame(Game game, DateTime? value);

        public abstract void EmptyFieldInGame(Game game);
    }
}