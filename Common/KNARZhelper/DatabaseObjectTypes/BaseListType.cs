using System;
using System.Collections.Generic;
using System.Linq;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseListType : BaseObjectType
    {
        public override bool IsList => true;

        public override bool AddDbObjectToGame(Game game, List<Guid> idList) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            (GameGuids(game, true) ?? (new List<Guid>())).AddMissing(idList));

        public override bool AddDbObjectToGame(Game game, Guid id) => API.Instance.MainView.UIDispatcher.Invoke(() =>
            (GameGuids(game, true) ?? (new List<Guid>())).AddMissing(id));

        public override bool DbObjectInGame(Game game, Guid id) => GameGuids(game).Contains(id);

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => GameGuids(x).Contains(id));

        public override bool DbObjectInUse(List<Game> games, Guid id) => games.Any(x => GameGuids(x).Contains(id));

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game, true).Clear());

        public override bool FieldInGameIsEmpty(Game game) => GameGuids(game).Count == 0;

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && GameGuids(g).Contains(id));

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && GameGuids(g).Contains(id)).ToList();

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids) => ids.Count != 0 && ids.Aggregate(false, (current, id) =>
            current | API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game).Remove(id)));

        public override bool RemoveObjectFromGame(Game game, Guid id) =>
            API.Instance.MainView.UIDispatcher.Invoke(() => GameGuids(game).Remove(id));

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, FieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (Game game in games.Where(g => GameGuids(g).Contains(id)))
            {
                if (!RemoveObjectFromGame(game, id))
                {
                    continue;
                }

                if (newType != null && newId != null)
                {
                    AddDbObjectToGame(game, (Guid)newId);
                }

                API.Instance.MainView.UIDispatcher.Invoke(delegate
                {
                    API.Instance.Database.Games.Update(game);
                });

                yield return game.Id;
            }

            if (removeAfter && !DbObjectInUse(id))
            {
                RemoveDbObject(id, false);
            }
        }

        internal abstract List<Guid> GameGuids(Game game, bool writeable = false);

        internal List<DatabaseObject> LoadGameMetadata<T>(List<T> items) where T : DatabaseObject =>
            items?.Select(x => new DatabaseObject() { Name = x.Name, Id = x.Id }).ToList() ?? new List<DatabaseObject>();

        internal List<DatabaseObject> LoadUnusedMetadata<T>(bool ignoreHiddenGames, IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Where(x => !API.Instance.Database.Games.Any(g => !(ignoreHiddenGames && g.Hidden) && GameGuids(g).Contains(x.Id)))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();
    }
}