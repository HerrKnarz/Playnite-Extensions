using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public IEnumerable<Guid> Merge(List<Game> games = null, List<MetadataObject> itemsToRemove = null)
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

                    result.AddMissing(item.ReplaceInDb(games, Type, Id, itemsToRemove == null));

                    if (itemsToRemove == null)
                    {
                        continue;
                    }

                    if (!itemsToRemove.Any(x => x.Type == item.Type && x.Name == item.Name))
                    {
                        itemsToRemove.Add(item);
                    }
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