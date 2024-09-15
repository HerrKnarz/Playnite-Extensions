using KNARZhelper.Enum;
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

        public override bool DbObjectInUse(Guid id) => API.Instance.Database.Games.Any(x => GetValue(x) == id);

        public override void EmptyFieldInGame(Game game) => API.Instance.MainView.UIDispatcher.Invoke(() => SetValue(game, default));

        public override bool FieldInGameIsEmpty(Game game) => GetValue(game) == default;

        public override int GetGameCount(List<Game> games, Guid id, bool ignoreHidden = false) =>
            games.Count(g => !(ignoreHidden && g.Hidden) && GetValue(g) == id);

        public override List<Game> GetGames(Guid id, bool ignoreHidden = false) =>
            API.Instance.Database.Games.Where(g => !(ignoreHidden && g.Hidden) && GetValue(g) == id).ToList();

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

        public override IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, FieldType? newType = null, Guid? newId = null, bool removeAfter = true)
        {
            if (id == Guid.Empty)
            {
                yield break;
            }

            IEditableObjectType newTypeManager =
                newType != null && newType != Type &&
                newType.Value.GetTypeManager() is IEditableObjectType editableObjectType
                    ? editableObjectType
                    : this;

            foreach (Game game in games.Where(g => GetValue(g) == id))
            {
                if (!RemoveObjectFromGame(game, id))
                {
                    continue;
                }

                if (newType != null && newId != null)
                {
                    newTypeManager.AddValueToGame(game, (Guid)newId);
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

        internal abstract Guid GetValue(Game game);

        internal List<DatabaseObject> LoadGameMetadata<T>(T item) where T : DatabaseObject =>
            new List<DatabaseObject> { new DatabaseObject() { Name = item.Name, Id = item.Id } };

        internal List<DatabaseObject> LoadUnusedMetadata<T>(bool ignoreHiddenGames, IItemCollection<T> collection) where T : DatabaseObject =>
            collection.Where(x => !API.Instance.Database.Games.Any(g => !(ignoreHiddenGames && g.Hidden) && GetValue(g) == x.Id))
            .Select(x => new DatabaseObject() { Id = x.Id, Name = x.Name }).ToList();

        internal abstract void SetValue(Game game, Guid id);
    }
}