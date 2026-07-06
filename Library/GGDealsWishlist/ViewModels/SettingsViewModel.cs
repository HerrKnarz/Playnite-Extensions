using GGDealsWishlist.Models;
using KNARZhelper;
using KNARZhelper.MetadataCommon;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace GGDealsWishlist.ViewModels
{
    public class SettingsViewModel : ObservableObject, ISettings
    {
        private readonly GGDealsWishlist plugin;
        private Settings settings;

        public SettingsViewModel(GGDealsWishlist plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite
            // saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved settings.
            var savedSettings = plugin.LoadPluginSettings<Settings>();

            // LoadPluginSettings returns null if no saved data is available.
            Settings = savedSettings ?? new Settings();
        }

        public static RelayCommand<object> RestartRequiredCommand => new RelayCommand<object>((sender) =>
        {
            try
            {
                var winParent = MiscHelper.FindParent<Window>((FrameworkElement)sender);

                if (winParent.DataContext?.GetType().GetProperty("IsRestartRequired") != null)
                {
                    ((dynamic)winParent.DataContext).IsRestartRequired = true;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        });

        public RelayCommand AddCategoryCommand => new RelayCommand(() => AddCategory());

        public Visibility GGDealsCollectionUpdaterWarningVisibility => IsGGDealsCollectionUpdaterInstalled ? Visibility.Visible : Visibility.Collapsed;

        public ImageOptionWithCaptions ImageOptionWithCaptions { get; } = new ImageOptionWithCaptions();

        public RelayCommand SetHistoricalLowColorCommand => new RelayCommand(() => SetHistoricalLowColor());

        public Settings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        internal static bool IsGGDealsCollectionUpdaterInstalled => API.Instance.Addons.Plugins.Exists(p => p.Id == Guid.Parse("2af05ded-085c-426b-a10e-8e03185092bf"));

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

        private void AddCategory()
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
        }

        private void SetHistoricalLowColor()
        {
            var colorDialog = new ColorDialog();

            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                var color = colorDialog.Color;

                Settings.HistoricalLowColorHex = $"#FF{color.R:X2}{color.G:X2}{color.B:X2}";
            }
        }
    }
}
