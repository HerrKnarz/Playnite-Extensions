using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System;

namespace WikipediaMetadata.Test.Fakes;

public class FakePlayniteApi : IPlayniteAPI
{
    public string ExpandGameVariables(Game game, string inputString) => throw new NotImplementedException();

    public string ExpandGameVariables(Game game, string inputString, string emulatorDir) => throw new NotImplementedException();

    public GameAction ExpandGameVariables(Game game, GameAction action) => throw new NotImplementedException();

    public void StartGame(Guid gameId) => throw new NotImplementedException();

    public void InstallGame(Guid gameId) => throw new NotImplementedException();

    public void UninstallGame(Guid gameId) => throw new NotImplementedException();

    public void AddCustomElementSupport(Plugin source, AddCustomElementSupportArgs args) => throw new NotImplementedException();

    public void AddSettingsSupport(Plugin source, AddSettingsSupportArgs args) => throw new NotImplementedException();

    public void AddConvertersSupport(Plugin source, AddConvertersSupportArgs args) => throw new NotImplementedException();

    public IMainViewAPI MainView { get; set; }
    public IGameDatabaseAPI Database { get; set; }
    public IDialogsFactory Dialogs { get; set; }
    public IPlaynitePathsAPI Paths { get; set; }
    public INotificationsAPI Notifications { get; set; }
    public IPlayniteInfoAPI ApplicationInfo { get; set; }
    public IWebViewFactory WebViews { get; set; }
    public IResourceProvider Resources { get; set; }
    public IUriHandlerAPI UriHandler { get; set; }
    public IPlayniteSettingsAPI ApplicationSettings { get; set; }
    public IAddons Addons { get; set; }
    public IEmulationAPI Emulation { get; set; }
}
