using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyCompanion
{
    public class CompanyCompanionSettings : ObservableObject
    {
        private bool showGroupKey = false;
        private ObservableCollection<string> businessEntityDescriptors;
        private ObservableCollection<string> ignoreWords;
        public bool ShowGroupKey { get => showGroupKey; set => SetValue(ref showGroupKey, value); }
        public ObservableCollection<string> BusinessEntityDescriptors { get => businessEntityDescriptors; set => SetValue(ref businessEntityDescriptors, value); }
        public ObservableCollection<string> IgnoreWords { get => ignoreWords; set => SetValue(ref ignoreWords, value); }
    }

    public class CompanyCompanionSettingsViewModel : ObservableObject, ISettings
    {
        private readonly CompanyCompanion plugin;
        private CompanyCompanionSettings EditingClone { get; set; }

        private CompanyCompanionSettings settings;

        public RelayCommand AddBusinessEntityDescriptorCommand
        {
            get => new RelayCommand(() =>
            {
                string value = API.Instance.Dialogs.SelectString("", "Add value", "").SelectedString;

                Settings.BusinessEntityDescriptors.AddMissing(value);
                Settings.BusinessEntityDescriptors = new ObservableCollection<string>(Settings.BusinessEntityDescriptors.OrderBy(x => x));
            });
        }

        public RelayCommand<IList<object>> RemoveBusinessEntityDescriptorCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (string item in items.ToList().Cast<string>())
                {
                    Settings.BusinessEntityDescriptors.Remove(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        public RelayCommand AddIgnoreWordCommand
        {
            get => new RelayCommand(() =>
            {
                string value = API.Instance.Dialogs.SelectString("", "Add value", "").SelectedString;

                Settings.IgnoreWords.AddMissing(value);
                Settings.IgnoreWords = new ObservableCollection<string>(Settings.IgnoreWords.OrderBy(x => x));
            });
        }

        public RelayCommand<IList<object>> RemoveIgnoreWordCommand
        {
            get => new RelayCommand<IList<object>>((items) =>
            {
                foreach (string item in items.ToList().Cast<string>())
                {
                    Settings.IgnoreWords.Remove(item);
                }
            }, (items) => items != null && items.Count > 0);
        }

        internal void SortCollection(ref ObservableCollection<string> collection)
        {
            collection = new ObservableCollection<string>(collection.OrderBy(x => x));
        }

        public CompanyCompanionSettings Settings
        {
            get => settings;
            set
            {
                settings = value;
                OnPropertyChanged();
            }
        }

        public CompanyCompanionSettingsViewModel(CompanyCompanion plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this.plugin = plugin;

            // Load saved Settings.
            CompanyCompanionSettings savedSettings = plugin.LoadPluginSettings<CompanyCompanionSettings>();

            // LoadPluginSettings returns null if no saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new CompanyCompanionSettings();
            }

            if (Settings.BusinessEntityDescriptors == null)
            {
                Settings.BusinessEntityDescriptors = new ObservableCollection<string>()
                {
                    "Co",
                    "Corp",
                    "GmbH",
                    "Inc",
                    "LLC",
                    "Ltd",
                    "srl",
                    "sro",
                };
            }
            else
            {
                Settings.BusinessEntityDescriptors = new ObservableCollection<string>(Settings.BusinessEntityDescriptors.OrderBy(x => x));
            }

            if (Settings.IgnoreWords == null)
            {
                Settings.IgnoreWords = new ObservableCollection<string>()
                {
                    "Corporation",
                    "Digital",
                    "Entertainment",
                    "Games",
                    "Interactive",
                    "Multimedia",
                    "Productions",
                    "Publishing",
                    "Software",
                    "Studios",
                    "The",
                };
            }
            else
            {
                Settings.IgnoreWords = new ObservableCollection<string>(Settings.IgnoreWords.OrderBy(x => x));
            }
        }

        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        public void EndEdit()
        {
            plugin.SavePluginSettings(Settings);
        }

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}