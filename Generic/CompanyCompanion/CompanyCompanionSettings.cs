using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace CompanyCompanion
{
    public class CompanyCompanionSettings : ObservableObject
    {
        private bool _showGroupKey = false;
        private ObservableCollection<string> _businessEntityDescriptors;
        private ObservableCollection<string> _ignoreWords;

        /// <summary>
        /// Defines, if the group key will be shown in the _merge companies window.
        /// </summary>
        public bool ShowGroupKey { get => _showGroupKey; set => SetValue(ref _showGroupKey, value); }
        /// <summary>
        /// List of words that will be removed from the games as business entity descriptors.
        /// </summary>
        public ObservableCollection<string> BusinessEntityDescriptors { get => _businessEntityDescriptors; set => SetValue(ref _businessEntityDescriptors, value); }
        /// <summary>
        /// List of words than will be ignored when searching for similar companies.
        /// </summary>
        public ObservableCollection<string> IgnoreWords { get => _ignoreWords; set => SetValue(ref _ignoreWords, value); }
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
                string value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCCompanyCompanionDialogAddValue"), "").SelectedString;

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
                string value = API.Instance.Dialogs.SelectString("", ResourceProvider.GetString("LOCCompanyCompanionDialogAddValue"), "").SelectedString;

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
            // Injecting your _plugin instance is required for Save/Load method because Playnite saves data to a location based on what _plugin requested the operation.
            this.plugin = plugin;

            // Load saved Settings.
            CompanyCompanionSettings savedSettings = plugin.LoadPluginSettings<CompanyCompanionSettings>();

            // LoadPluginSettings returns null if no saved data is available.

            Settings = savedSettings ?? new CompanyCompanionSettings();

            if (Settings.BusinessEntityDescriptors is null)
            {
                Settings.BusinessEntityDescriptors = new ObservableCollection<string>()
                {
                    "AB",
                    "ACE",
                    "Co",
                    "Co.,Ltd.",
                    "Corp",
                    "GmbH",
                    "Inc",
                    "LLC",
                    "Ltd",
                    "Pte.",
                    "Pty",
                    "S.A.",
                    "S.L.",
                    "s.r.o.",
                    "srl",
                };
            }
            else
            {
                Settings.BusinessEntityDescriptors = new ObservableCollection<string>(Settings.BusinessEntityDescriptors.OrderBy(x => x));
            }

            if (Settings.IgnoreWords is null)
            {
                Settings.IgnoreWords = new ObservableCollection<string>()
                {
                    "Corporation",
                    "Digital",
                    "Entertainment",
                    "Games",
                    "Interactive",
                    "Media",
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