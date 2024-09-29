using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public abstract class BaseSingleObjectType : BaseObjectType
    {
        public override bool IsList => false;

        public override bool AddValueToGame<T>(Game game, T value)
        {
            switch (value)
            {
                case Guid id:
                    if (GetValue(game) == id)
                    {
                        return false;
                    }

                    API.Instance.MainView.UIDispatcher.Invoke(() => SetValue(game, id));

                    return true;

                default:
                    return false;
            }
        }

        public override bool DbObjectInGame(Game game, Guid id) => GetValue(game) == id;

        public override bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Any(x => !(ignoreHiddenGames && x.Hidden) && GetValue(x) == id);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => SetValue(game, default));

        public override bool FieldInGameIsEmpty(Game game) => GetValue(game) == default;

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHiddenGames = false) =>
            games.Count(g => !(ignoreHiddenGames && g.Hidden) && GetValue(g) == id);

        public override List<Game> GetGames(Guid id, bool ignoreHiddenGames = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHiddenGames && g.Hidden) && GetValue(g) == id).ToList();

        public override bool RemoveObjectFromGame(Game game, List<Guid> ids)
        {
            if (ids.Count == 0 || !ids.Contains(GetValue(game)))
            {
                return false;
            }

            EmptyFieldInGame(game);

            return true;
        }

        public override bool RemoveObjectFromGame(Game game, Guid id)
        {
            if (GetValue(game) != id)
            {
                return false;
            }

            EmptyFieldInGame(game);

            return true;
        }

        public override IEnumerable<Guid> RemoveObjectFromGames(List<Game> games, Guid id)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (var game in games.Where(game => GetValue(game) == id))
            {
                EmptyFieldInGame(game);

                yield return game.Id;
            }
        }

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, IEditableObjectType newType = null, Guid? newId = null)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            foreach (var game in games.Where(g => GetValue(g) == id))
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

        internal abstract Guid GetValue(Game game);

        internal List<DatabaseObject> LoadGameMetadata<T>(T item) where T : DatabaseObject =>
            new List<DatabaseObject> { new DatabaseObject() { Name = item.Name, Id = item.Id } };

        internal List<DatabaseObject> LoadUnusedMetadata<T>(bool ignoreHiddenGames, IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Where(x => !DbObjectInUse(x.Id, ignoreHiddenGames)).Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        internal abstract void SetValue(Game game, Guid id);
    }
}