using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using KNARZhelper.MetadataCommon.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using ScreenshotUtilities.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace ScreenshotUtilities
{
    public class ScreenshotUtilitiesSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ScreenshotUtilities plugin;
        private Settings EditingClone { get; set; }

        private Settings settings;
        public Settings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public ScreenshotUtilitiesSettingsViewModel(ScreenshotUtilities plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<Settings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new Settings();
            }
        }

        public RelayCommand AddCategoryCommand => new RelayCommand(() => SelectMetadata(FieldType.Category));

        public RelayCommand AddTagCommand => new RelayCommand(() => SelectMetadata(FieldType.Tag));

        public RelayCommand<IList<object>> RemoveFromListCommand => new RelayCommand<IList<object>>(items =>
            {
                if (items == null || items.Count == 0)
                {
                    return;
                }

                foreach (var item in items.ToList().Cast<MetadataObject>())
                {
                    Settings.DownloadFilter.Remove(item);
                }
            }, items => items?.Count != 0);

        public void SelectMetadata(FieldType type)
        {
            if (type != FieldType.Category && type != FieldType.Tag)
            {
                Debug.WriteLine("ScreenshotUtilitiesSettingsViewModel.SelectMetadata: Invalid field type");
                return;
            }

            BaseListType typeManager = new TypeTag();

            if (type == FieldType.Category)
            {
                typeManager = new TypeCategory();
            }
            else if (type == FieldType.Tag)
            {
                typeManager = new TypeTag();
            }

            var label = typeManager.LabelPlural;

            var items = new ObservableCollection<BaseMetadataObject>();

            typeManager.LoadAllMetadata(new HashSet<System.Guid>()).ForEach(item => items.Add(
                            new BaseMetadataObject(typeManager, typeManager.Type, item.Name)
                            {
                                Id = item.Id
                            }));

            SelectMetadataViewModel.GetWindow(items, label)?.ShowDialog();

            if (items.Count == 0)
            {
                return;
            }

            foreach (var item in items.Where(item => item.Selected && Settings.DownloadFilter.All(x => x.TypeAndName != item.TypeAndName)))
            {
                Settings.DownloadFilter.Add(new MetadataObject(item.Type, item.Name));
            }

            Settings.DownloadFilter.Sort(x => x.TypeAndName);
        }

        public void BeginEdit() =>
            // Code executed when settings view is opened and user starts editing values.
            EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() =>
            // Code executed when user decides to cancel any changes made since BeginEdit was called.
            // This method should revert any changes made to Option1 and Option2.
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