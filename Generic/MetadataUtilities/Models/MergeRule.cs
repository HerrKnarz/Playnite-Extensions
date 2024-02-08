using KNARZhelper;
using MetadataUtilities.Actions;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.ObjectModel;

namespace MetadataUtilities.Models
{
    public class MergeRule : MetadataListObject, IBaseAction
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

        public string ProgressMessage => "LOCMetadataUtilitiesDialogMergingItems";

        public string ResultMessage => "LOCMetadataUtilitiesDialogMergedMetadataMessage";

        public bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => Merge(game, isBulkAction, false);

        public bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true) => GetIds();

        public bool GetIds()
        {
            Id = DatabaseObjectHelper.AddDbObject(Type, Name);

            foreach (MetadataListObject item in SourceObjects)
            {
                item.Id = DatabaseObjectHelper.GetDbObjectId(item.Name, item.Type);
            }

            return true;
        }

        public bool Merge(Game game, bool isPrepared = false, bool updateGame = true)
        {
            if (!isPrepared)
            {
                GetIds();
            }

            bool needToAdd = false;

            foreach (MetadataListObject item in SourceObjects)
            {
                if (item.Id != Id && item.Id != Guid.Empty)
                {
                    needToAdd |= DatabaseObjectHelper.RemoveObjectFromGame(game, item.Type, item.Id);
                }
            }

            if (!needToAdd)
            {
                return false;
            }

            DatabaseObjectHelper.AddDbObjectToGame(game, Type, Id);

            if (updateGame)
            {
                API.Instance.Database.Games.Update(game);
            }

            return needToAdd;
        }
    }
}