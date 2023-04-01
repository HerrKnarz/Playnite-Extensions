using Playnite.SDK;
using Playnite.SDK.Data;
using System.Collections.Generic;

namespace KNARZtools
{
    // ReSharper disable once InconsistentNaming
    public class KNARZtoolsSettings : ObservableObject
    {
        private string _option1 = string.Empty;
        private bool _option2 = false;
        private bool _optionThatWontBeSaved = false;

        public string Option1
        {
            get => _option1;
            set => SetValue(ref _option1, value);
        }

        public bool Option2
        {
            get => _option2;
            set => SetValue(ref _option2, value);
        }

        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        [DontSerialize]
        public bool OptionThatWontBeSaved
        {
            get => _optionThatWontBeSaved;
            set => SetValue(ref _optionThatWontBeSaved, value);
        }
    }

    // ReSharper disable once InconsistentNaming
    public class KNARZtoolsSettingsViewModel : ObservableObject, ISettings
    {
        private readonly KNARZtools _plugin;
        private KNARZtoolsSettings EditingClone { get; set; }

        private KNARZtoolsSettings _settings;

        public KNARZtoolsSettings Settings
        {
            get => _settings;
            set
            {
                _settings = value;
                OnPropertyChanged();
            }
        }

        public KNARZtoolsSettingsViewModel(KNARZtools plugin)
        {
            _plugin = plugin;

            KNARZtoolsSettings savedSettings = plugin.LoadPluginSettings<KNARZtoolsSettings>();

            Settings = savedSettings ?? new KNARZtoolsSettings();
        }

        public void BeginEdit() =>
            EditingClone = Serialization.GetClone(Settings);

        public void CancelEdit() =>
            Settings = EditingClone;

        public void EndEdit() =>
            _plugin.SavePluginSettings(Settings);

        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}