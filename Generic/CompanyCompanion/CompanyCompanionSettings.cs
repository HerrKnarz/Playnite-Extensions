using KNARZhelper;
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
        private readonly CompanyCompanion _plugin;
        private CompanyCompanionSettings EditingClone { get; set; }

        private CompanyCompanionSettings _settings;

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
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public CompanyCompanionSettingsViewModel(CompanyCompanion plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            this._plugin = plugin;

            // Load saved Settings.
            CompanyCompanionSettings savedSettings = plugin.LoadPluginSettings<CompanyCompanionSettings>();

            // LoadPluginSettings returns null if no saved data is available.

            Settings = savedSettings ?? new CompanyCompanionSettings();

            // Since the addon first had all built in describers in a editable list, we remove those for newer versions of the addon.
            List<string> existintValues = new List<string>()
                {
                    "ab",
                    "ace",
                    "co",
                    "coltd",
                    "corp",
                    "gmbh",
                    "inc",
                    "llc",
                    "ltd",
                    "pte",
                    "pty",
                    "sa",
                    "sl",
                    "sro",
                    "srl",
                };

            Settings.BusinessEntityDescriptors = Settings.BusinessEntityDescriptors is null
                ? new ObservableCollection<string>()
                : new ObservableCollection<string>(Settings.BusinessEntityDescriptors
                    .Where(d => !existintValues.Contains(d.RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", ""))).OrderBy(x => x).ToList());

            Settings.IgnoreWords = Settings.IgnoreWords is null
                ? new ObservableCollection<string>()
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
                }
                : new ObservableCollection<string>(Settings.IgnoreWords.OrderBy(x => x).ToList());
        }

        public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() => Settings = EditingClone;

        public void EndEdit() => _plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}