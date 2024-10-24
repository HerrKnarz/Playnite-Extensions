using KNARZhelper;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataObject
    {
        private MetadataObjects _sourceObjects;

        public MergeRule(FieldType type, string name = default) : base(type, name) =>
            _sourceObjects = new MetadataObjects();

        public MetadataObjects SourceObjects
        {
            get => _sourceObjects;
            set
            {
                _sourceObjects = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Guid> Merge(List<Game> games = null, bool removeAfter = true)
        {
            var result = new List<Guid>();

            AddToDb();

            try
            {
                foreach (var item in SourceObjects)
                {
                    if (item.Id == Id || item.Id == default)
                    {
                        continue;
                    }

                    if (games == null)
                    {
                        games = API.Instance.Database.Games.ToList();
                    }

                    result.AddMissing(item.ReplaceInDb(games, Type, Id));
                }

                MetadataFunctions.UpdateGames(result);

                if (removeAfter)
                {
                    foreach (var item in SourceObjects.Where(x =>
                                 x.Id != Id && x.Id != default))
                    {
                        item.RemoveFromDb();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return result;
        }

        public bool Merge(Game game, bool needToAdd = false)
        {
            if (needToAdd)
            {
                AddToDb();
            }

            var result = false;

            try
            {
                result = SourceObjects.Where(item => item.Id != Id && item.Id != default)
                    .Aggregate(false, (current, item) => current | item.RemoveFromGame(game));

                if (result)
                {
                    AddToGame(game);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }

            return result;
        }
    }
}