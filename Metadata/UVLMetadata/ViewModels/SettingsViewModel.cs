using KNARZhelper;
using Playnite.SDK;
using Playnite.SDK.Data;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UVLMetadata.Enums;
using UVLMetadata.Models;

namespace UVLMetadata.ViewModels;

public class SettingsViewModel : ObservableObject, ISettings
{
    private readonly UVLMetadata _plugin;
    private RelayCommand _authenticateCommand;
    private RelayCommand _refreshTagsCommand;
    private RelayCommand<object> _restartRequiredCommand;

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

    public AddLinkModes AddLinkModes { get; } = [];

    public ICommand AuthenticateCommand => _authenticateCommand ??= new RelayCommand(Authenticate);

    public string AuthenticationButtonText => AuthenticationStatusButtonModes[IsAuthenticated];

    public string AuthenticationStatusText => AuthenticationStatusModes[IsAuthenticated];

    public DescriptionToUseModes DescriptionToUseModes { get; } = [];

    public Dictionary<MetadataField, string> ImportAsModes { get; } = new()
    {
        { MetadataField.Features, ResourceProvider.GetString("LOCFeaturesLabel") },
        { MetadataField.Genres, ResourceProvider.GetString("LOCGenresLabel") },
        { MetadataField.Tags, ResourceProvider.GetString("LOCTagsLabel") }
    };

    public AuthenticationStatus IsAuthenticated
    {
        get
        {
            if (field == AuthenticationStatus.Unknown)
            {
                field = _plugin.UVLConnect.IsUserLoggedIn();
            }

            return field;
        }
        set
        {
            SetValue(ref field, value);
            OnPropertyChanged(nameof(AuthenticationButtonText));
            OnPropertyChanged(nameof(AuthenticationStatusText));
        }
    } = AuthenticationStatus.Unknown;

    public RatingToUseModes RatingToUseModes { get; } = [];
    public ICommand RefreshTagsCommand => _refreshTagsCommand ??= new RelayCommand(RefreshTags);

    public ICommand RestartRequiredCommand => _restartRequiredCommand ??= new RelayCommand<object>(RestartRequired);

    public PluginSettings Settings { get; private set; }

    private AuthenticationStatusButtonModes AuthenticationStatusButtonModes { get; } = [];
    private AuthenticationStatusModes AuthenticationStatusModes { get; } = [];
    private PluginSettings EditingClone { get; set; }

    public void BeginEdit() => EditingClone = Serialization.GetClone(Settings);

    public void CancelEdit()
    {
        Settings = EditingClone;
        PrepareTagCategories();
    }

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

    private void Authenticate()
    {
        if (IsAuthenticated == AuthenticationStatus.Authenticated)
        {
            System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;

            try
            {
                IsAuthenticated = _plugin.UVLConnect.Logout();
            }
            finally
            {
                System.Windows.Forms.Cursor.Current = System.Windows.Forms.Cursors.Default;
            }
        }
        else
        {
            IsAuthenticated = _plugin.UVLConnect.Authenticate();
        }
    }

    private void RefreshTags()
    {
        _plugin.UVLConnect.RefreshTags();
        Settings.LastTagRefresh = _plugin.Tags.LastRefresh;
    }

    private void RestartRequired(object sender)
    {
        try
        {
            var winParent = MiscHelper.FindParent<Window>((FrameworkElement)sender);

            if (winParent?.DataContext?.GetType().GetProperty("IsRestartRequired") != null)
            {
                ((dynamic)winParent.DataContext).IsRestartRequired = true;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex);
        }
    }
}
