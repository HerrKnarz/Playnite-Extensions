using KNARZhelper;
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

        public MergeRule(Settings settings) : base(settings) => _sourceObjects = new MetadataObjects(settings);

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
            List<Guid> result = new List<Guid>();

            AddToDb();

            try
            {
                foreach (MetadataObject item in SourceObjects)
                {
                    if (item.Id == Id || item.Id == default)
                    {
                        continue;
                    }

                    if (games == null)
                    {
                        games = API.Instance.Database.Games.ToList();
                    }

                    result.AddMissing(item.ReplaceInDb(games, Type, Id, removeAfter));
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

            bool result = false;

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