using KNARZhelper;
using Playnite.SDK;
using System;
using System.Collections.ObjectModel;

namespace AdvancedMetadataTools.Models
{
    public class MetadataListObjects : ObservableCollection<MetadataListObject>
    {
        public bool MergeItems(FieldType type, Guid id)
        {
            bool result = false;

            using (API.Instance.Database.BufferedUpdate())
            {
                GlobalProgressOptions globalProgressOptions = new GlobalProgressOptions(
                    ResourceProvider.GetString("LOCAdvancedMetadataToolsDialogMergingItems"),
                    false
                )
                {
                    IsIndeterminate = false
                };

                API.Instance.Dialogs.ActivateGlobalProgress(activateGlobalProgress =>
                {
                    try
                    {
                        activateGlobalProgress.ProgressMaxValue = Count - 1;

                        foreach (MetadataListObject item in this)
                        {
                            if (item.Id != id)
                            {
                                DatabaseObjectHelper.ReplaceDbObject(item.Type, item.Id, type, id);

                                activateGlobalProgress.CurrentProgressValue++;
                            }
                        }

                        result = true;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex);
                    }
                }, globalProgressOptions);
            }

            return result;
        }
    }
}