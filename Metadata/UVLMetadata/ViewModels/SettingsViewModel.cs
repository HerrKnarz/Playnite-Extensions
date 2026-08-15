using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using UVLMetadata.Models;

namespace UVLMetadata.ViewModels;

public class SettingsViewModel : ObservableObject, ISettings
{
    private readonly UVLMetadata _plugin;

    private RelayCommand authenticateCommand;
    private RelayCommand refreshTagsCommand;

    public SettingsViewModel(UVLMetadata plugin)
    {
        // Injecting your plugin instance is required for Save/Load method because Playnite saves
        // data to a location based on what plugin requested the operation.
        _plugin = plugin;

        // LoadPluginSettings returns null if no saved data is available.
        Settings = plugin.LoadPluginSettings<PluginSettings>() ?? new PluginSettings();

        Settings.LastTagRefresh = plugin.Tags.LastRefresh;

        PrepareTagCategories();

        CheckAuthenticationStatus();
    }

    public ICommand AuthenticateCommand => authenticateCommand ??= new RelayCommand(Authenticate);

    public string AuthenticationStatusText => IsAuthenticated switch
    {
        AuthenticationStatus.Authenticated => ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusAuthenticated"),
        AuthenticationStatus.NotAuthenticated => ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusNotAuthenticated"),
        _ => ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusCheckingStatus")
    };

    public Dictionary<DescriptionToUse, string> DescriptionToUseModes { get; } = new()
    {
        { DescriptionToUse.OfficialDescription, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseOfficialDescription") },
        { DescriptionToUse.Description, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseDescription") },
        { DescriptionToUse.Both, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseBoth") }
    };

    public Dictionary<MetadataField, string> ImportAsModes { get; } = new()
    {
        { MetadataField.Features, ResourceProvider.GetString("LOCFeaturesLabel") },
        { MetadataField.Genres, ResourceProvider.GetString("LOCGenresLabel") },
        { MetadataField.Tags, ResourceProvider.GetString("LOCTagsLabel") }
    };

    public AuthenticationStatus IsAuthenticated
    {
        get;
        set => SetValue(ref field, value);
    } = AuthenticationStatus.Unknown;

    public Dictionary<RatingToUse, string> RatingToUseModes { get; } = new()
    {
        { RatingToUse.Median, ResourceProvider.GetString("LOCUVLMetadataSettingsRatingMedian") },
        { RatingToUse.Average, ResourceProvider.GetString("LOCUVLMetadataSettingsRatingAverage") }
    };

    public ICommand RefreshTagsCommand => refreshTagsCommand ??= new RelayCommand(RefreshTags);

    public PluginSettings Settings { get; private set; }

    private PluginSettings EditingClone { get; set; }

    public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

    public void CancelEdit() => Settings = EditingClone;

    public async void CheckAuthenticationStatus()
    {
        IsAuthenticated = _plugin.UVLConnect.IsUserLoggedIn();
        await Task.FromResult(0);
    }

    public void EndEdit() => _plugin.SavePluginSettings(Settings);

    public void PrepareTagCategories()
    {
        if (Settings.TagCategories == null)
        {
            Settings.TagCategories = [];

            return;
        }

        var defaultCategories = new TagCategories();

        foreach (var category in defaultCategories)
        {
            if (!Settings.TagCategories.ContainsKey(category.Key))
            {
                Settings.TagCategories.Add(category.Key, category.Value);
                continue;
            }

            var existingCategory = Settings.TagCategories[category.Key];

            existingCategory.TranslationResourceKey = category.Value.TranslationResourceKey;
            existingCategory.Caption = category.Value.Caption;
            existingCategory.Url = category.Value.Url;
        }
    }

    public bool VerifySettings(out List<string> errors)
    {
        errors = [];
        return true;
    }

    private void Authenticate() => IsAuthenticated = _plugin.UVLConnect.Authenticate();

    private void RefreshTags()
    {
        _plugin.UVLConnect.RefreshTags();
        Settings.LastTagRefresh = _plugin.Tags.LastRefresh;
    }
}
