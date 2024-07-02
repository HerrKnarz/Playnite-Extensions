using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataObject
    {
        private MetadataObjects _sourceObjects;

        public MergeRule(Settings settings) : base(settings)
        {
            _sourceObjects = new MetadataObjects(settings);
        }

        public MetadataObjects SourceObjects
        {
            get => _sourceObjects;
            set
            {
                _sourceObjects = value;
                OnPropertyChanged();
            }
        }

        public bool GetIds()
        {
            Id = DatabaseObjectHelper.AddDbObject(Type, Name);

            foreach (MetadataObject item in SourceObjects)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);
            }

            return true;
        }

        public IEnumerable<Guid> Merge(List<Game> games = null)
        {
            List<Guid> result = new List<Guid>();

            GetIds();

            try
            {
                foreach (MetadataObject item in SourceObjects)
                {
                    if (item.Id == Id)
                    {
                        continue;
                    }

                    if (games == null)
                    {
                        games = API.Instance.Database.Games.ToList();
                    }

                    result.AddMissing(DatabaseObjectHelper.ReplaceDbObject(games, item.Type, item.Id, Type, Id));
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