using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseListType : BaseObjectType
    {
        public override bool IsList => true;

        public override bool AddValueToGame<T>(Game game, T value)
        {
            switch (value)
            {
                case List<Guid> idList:
                    return API.Instance.MainView.UIDispatcher.Invoke(() =>
                        (GameGuids(game, true) ?? (new List<Guid>())).AddMissing(idList));

                case Guid id:
                    return API.Instance.MainView.UIDispatcher.Invoke(() =>
                        (GameGuids(game, true) ?? (new List<Guid>())).AddMissing(id));

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

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, FieldType? newType = null, Guid? newId = null)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            var newTypeManager =
                newType != null && newType != Type &&
                newType.Value.GetTypeManager() is IEditableObjectType editableObjectType
                    ? editableObjectType
                    : this;

            foreach (var game in games.Where(g => GameGuids(g).Contains(id)))
            {
                if (!RemoveObjectFromGame(game, id))
                {
                    continue;
                }

                if (newType != null && newId != null)
                {
                    newTypeManager.AddValueToGame(game, (Guid)newId);
                }

                yield return game.Id;
            }
        }

        internal abstract List<Guid> GameGuids(Game game, bool writeable = false);

        internal List<DatabaseObject> LoadGameMetadata<T>(List<T> items) where T : DatabaseObject =>
            items?.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        internal List<DatabaseObject> LoadUnusedMetadata<T>(bool ignoreHiddenGames, IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Where(x => !DbObjectInUse(x.Id, ignoreHiddenGames)).Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();
    }
}