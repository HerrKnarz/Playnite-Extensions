﻿using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Linq;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Base type for media metadata fields.
    /// </summary>
    public abstract class BaseMediaType : IMetadataFieldType, IValueType, IClearAbleType
    {
        public bool CanBeAdded => false;
        public bool CanBeClearedInGame => true;
        public bool CanBeDeleted => false;
        public bool CanBeEmptyInGame => true;
        public bool CanBeModified => false;
        public bool CanBeSetByMetadataAddOn => true;
        public bool CanBeSetInGame => true;
        public string LabelPlural => LabelSingular;
        public abstract string LabelSingular { get; }
        public abstract FieldType Type { get; }
        public ItemValueType ValueType => ItemValueType.Media;

        public bool IsDefaultToCopy => true;

        public bool AddValueToGame<T>(Game game, T value)
        {
            if (!(value is string filePath) || filePath.Length == 0)
            {
                return false;
            }

            EmptyFieldInGame(game);

            SetValue(game, filePath);

            return true;
        }

        public bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false)
        {
            if (sourceGame == null || targetGame == null)
            {
                return false;
            }

            if (!replaceValue && (!onlyIfEmpty || !FieldInGameIsEmpty(targetGame)))
            {
                return false;
            }

            if (FieldInGameIsEmpty(sourceGame))
            {
                if (FieldInGameIsEmpty(targetGame))
                {
                    return false;
                }

                EmptyFieldInGame(targetGame);

                return true;
            }
            else
            {
                return AddValueToGame(targetGame, API.Instance.Database.GetFullFilePath(GetValue(sourceGame)));
            }
        }

        public void EmptyFieldInGame(Game game)
        {
            if (FieldInGameIsEmpty(game))
            {
                return;
            }

            API.Instance.Database.RemoveFile(GetValue(game));
        }

        public bool FieldInGameIsEmpty(Game game) => !GetValue(game)?.Any() ?? true;

        public bool GameContainsValue<T>(Game game, T value) => !GetValue(game)?.Any() ?? true;

        /// <summary>
        /// Gets the media value of the field for the specified game. Can be null.
        /// </summary>
        /// <param name="game">Game to get the media value from</param>
        /// <returns>Media value of the field for the specified game</returns>
        internal abstract string GetValue(Game game);

        /// <summary>
        /// Sets the media value of the field for the specified game.
        /// </summary>
        /// <param name="game">Game to set the media value for</param>
        /// <param name="value">Media value to set</param>
        internal abstract void SetValue(Game game, string value);
    }
}