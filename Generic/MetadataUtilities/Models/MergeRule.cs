using KNARZhelper;
using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

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

                ControlCenter.UpdateGames(result);

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

        public void MergeItems(bool showDialog = true)
        {
            ControlCenter.Instance.IsUpdating = true;
            Cursor.Current = Cursors.WaitCursor;
            var gamesAffected = new List<Guid>();
            try
            {
                using (API.Instance.Database.BufferedUpdate())
                {
                    var globalProgressOptions = new GlobalProgressOptions(
                        ResourceProvider.GetString("LOCMetadataUtilitiesProgressMergingItems"),
                        false
                    )
                    {
                        IsIndeterminate = true
                    };

                    API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                    {
                        try
                        {
                            gamesAffected.AddMissing(Merge(API.Instance.Database.Games.ToList()));
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    }, globalProgressOptions);
                }
            }
            finally
            {
                ControlCenter.Instance.IsUpdating = false;
                Cursor.Current = Cursors.Default;
            }

            // Shows a dialog with the number of games actually affected.
            if (!showDialog)
            {
                return;
            }

            API.Instance.Dialogs.ShowMessage(string.Format(ResourceProvider.GetString("LOCMetadataUtilitiesDialogMergedMetadataMessage"), gamesAffected.Distinct().Count()));
        }
    }
}