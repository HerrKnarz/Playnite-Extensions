using GGDealsWishlist.Models;
using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GGDealsWishlist
{
    public class GGDealsWishlistSettingsViewModel : ObservableObject, ISettings
    {
        private readonly GGDealsWishlist plugin;
        private Settings settings;

        public GGDealsWishlistSettingsViewModel(GGDealsWishlist plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite
            // saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<Settings>();

            // LoadPluginSettings returns null if no saved data is available.
            Settings = savedSettings ?? new Settings();
        }

        public RelayCommand AddCategoryCommand
            => new RelayCommand(() =>
            {
                var typeManager = new TypeCategory();
                var label = typeManager.LabelPlural;
                var items = new ObservableCollection<BaseMetadataObject>();

                typeManager.LoadAllMetadata(new HashSet<System.Guid>()).ForEach(item => items.Add(
                                new BaseMetadataObject(typeManager, typeManager.Type, item.Name)
                                {
                                    Id = item.Id
                                }));

                items.Sort(i => i.Name);

                SelectMetadataViewModel.GetWindow(items, label, false)?.ShowDialog();

                if (items.Count(i => i.Selected) == 0)
                {
                    return;
                }

                Settings.DefaultCategory = items.First(i => i.Selected).Name;
            });

        public Settings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        private Settings EditingClone { get; set; }

        public void BeginEdit() =>
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() =>
            // Code executed when user decides to cancel any changes made since BeginEdit was
            // called. This method should revert any changes made to Option1 and Option2.
            Settings = EditingClone;

        public void EndEdit() =>
            // Code executed when user decides to confirm changes made since BeginEdit was called.
            // This method should save settings made to Option1 and Option2.
            plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            // Code execute when user decides to confirm changes made since BeginEdit was called.
            // Executed before EndEdit is called and EndEdit is not called if false is returned.
            // List of errors is presented to user if verification fails.
            errors = new List<string>();
            return true;
        }
    }
}
