using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.MetadataCommon.DatabaseObjectTypes
{
    /// <summary>
    /// Base type for list metadata fields.
    /// </summary>
    public abstract class BaseListType : BaseObjectType
    {
        public override bool IsList => true;

        public override bool AddValueToGame<T>(Game game, T value)
        {
            switch (value)
            {
                case List<Guid> idList:
                    return API.Instance.MainView.UIDispatcher.Invoke(() =>
                        (GameGuids(game, true) ?? new List<Guid>()).AddMissing(idList));

                case Guid id:
                    return API.Instance.MainView.UIDispatcher.Invoke(() =>
                        (GameGuids(game, true) ?? new List<Guid>()).AddMissing(id));

                default:
                    return false;
            }
        }

        public override bool DbObjectInGame(Game game, Guid id) => GameGuids(game).Contains(id);

        public override bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Any(x => !(ignoreHiddenGames && x.Hidden) && GameGuids(x).Contains(id));

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game, true).Clear());

        public override bool FieldInGameIsEmpty(Game game) => GameGuids(game).Count == 0;

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false) =>
            games.Count(g => !(ignoreHiddenGames && g.Hidden) && GameGuids(g).Contains(id));

        public override List<Game> GetGames(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden) && GameGuids(g).Contains(id)).ToList();

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
            current | API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game).Remove(id)));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game).Remove(id));

        public override IEnumerable<Guid> RemoveObjectFromGames(List<Game> games, Guid id)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (var game in games.Where(g => GameGuids(g).Contains(id)))
            {
                if (RemoveObjectFromGame(game, id))
                {
                    yield return game.Id;
                }
            }
        }

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, IEditableObjectType newType = null, Guid? newId = null)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (var game in games.Where(g => GameGuids(g).Contains(id)))
            {
                if (!RemoveObjectFromGame(game, id))
                {
                    continue;
                }

                if (newType != null && newId != null)
                {
                    newType.AddValueToGame(game, (Guid)newId);
                }

                yield return game.Id;
            }
        }

        /// <summary>
        /// List of GUIDs of the database objects of this type in the specified game.
        /// </summary>
        /// <param name="game">Game to get the GUIDs from</param>
        /// <param name="writeable">Determines if this should be a writeable copy to not change the values in the original list.</param>
        /// <returns></returns>
        internal abstract List<Guid> GameGuids(Game game, bool writeable = false);

        /// <summary>
        /// Loads the metadata objects of the specified game, ignoring the ones in the ignore list.
        /// </summary>
        /// <typeparam name="T">Type of the metadata object</typeparam>
        /// <param name="items">List of metadata objects to load</param>
        /// <param name="itemsToIgnore">Set of GUIDs to ignore</param>
        /// <returns>List of loaded metadata objects</returns>
        internal List<DatabaseObject> LoadGameMetadata<T>(List<T> items, HashSet<Guid> itemsToIgnore) where T : DatabaseObject =>
            items?.Where(x => !itemsToIgnore?.Contains(x.Id) ?? true).Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        /// <summary>
        /// Loads all unused metadata objects of this type in the database.
        /// </summary>
        /// <typeparam name="T">Type of the metadata object</typeparam>
        /// <param name="ignoreHiddenGames">Determines if hidden games should be ignored. When true items only used in hidden games will be considered unused.</param>
        /// <param name="collection">Collection of metadata objects to search</param>
        /// <returns>List of unused metadata objects</returns>
        internal List<DatabaseObject> LoadUnusedMetadata<T>(bool ignoreHiddenGames, IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Where(x => !DbObjectInUse(x.Id, ignoreHiddenGames)).Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        public override bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false)
        {
            if (sourceGame == null || targetGame == null)
            {
                return false;
            }

            var result = false;

            if (replaceValue || !onlyIfEmpty || FieldInGameIsEmpty(targetGame))
            {
                if (replaceValue && !FieldInGameIsEmpty(targetGame))
                {
                    result = true;

                    EmptyFieldInGame(targetGame);
                }

                result |= AddValueToGame(targetGame, LoadGameMetadata(sourceGame).Select(x => x.Id).ToList());
            }

            return result;
        }
    }
}