using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataListObject
    {
        private ObservableCollection<MetadataListObject> _sourceObjects = new ObservableCollection<MetadataListObject>();

        public ObservableCollection<MetadataListObject> SourceObjects
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

            foreach (MetadataListObject item in SourceObjects)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);
            }

            return true;
        }

        public IEnumerable<Guid> Merge(List<Game> games = null, bool removeUnused = true)
        {
            List<Guid> result = new List<Guid>();

            GetIds();

            try
            {
                foreach (MetadataListObject item in SourceObjects)
                {
                    if (item.Id == Id)
                    {
                        continue;
                    }

                    if (games == null)
                    {
                        games = API.Instance.Database.Games.ToList();
                    }

                    result.AddMissing(DatabaseObjectHelper.ReplaceDbObject(games, item.Type, item.Id, Type, Id, removeUnused));
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