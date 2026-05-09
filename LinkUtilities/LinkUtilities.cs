using LinkUtilities.DataHandlers;
using LinkUtilities.LinkActions;
using LinkUtilities.Linker;
using LinkUtilities.Models;
using LinkUtilities.ViewModels;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities;

public class LinkUtilitiesPlugin : Plugin
{
    public const string Id = "HerrKnarz.LinkUtilities";
    public const string LinkPropertyId = "LinkUtilities.LinkPropery";
    private HandleUriActions? _handleUriActions;
    public static string? InstallDir { get; set; }

    /// <summary>
    /// Is set to true, while the library is updated via one of the DoForAll methods. Is used to
    /// avoid endless loops caused by triggering the game update.
    /// </summary>
    /// NEXT: Check how to set this up in the base action after verifying that it actually works.
    public static bool IsUpdating { get; set; } = false;

    public static IPlayniteApi? PlayniteApi { get; private set; } = null!;
    public static LinkUtilitiesPlugin? Plugin { get; private set; } = null!;
    public static LinkUtilitiesPluginSettings Settings { get; set; } = new();
    public Links Links { get; set; } = [];
    public MenuHandler? MenuHandler { get; set; }

#pragma warning disable IDE0060 // Remove unused parameter

    public static async Task TestMethod(List<BaseActionGame> games)
#pragma warning restore IDE0060 // Remove unused parameter
    {
        return;
    }

    public override GameExplorer? GetGameExplorer(GetGameExplorersArgs args) => args.ItemId == LinkPropertyId ? new LinkPropertyGameExplorer() : (GameExplorer?)null;

    public override ICollection<GameExplorerDescriptor> GetGameExplorerDescriptors(GetGameExplorerDescriptorsArgs args)
        => [new GameExplorerDescriptor(LinkPropertyId, Loc.caption_link_type())];

    public override ICollection<GameFiltererDescriptor> GetGameFilterDescriptors(GetGameFiltereDescriptorsArgs args)
        => [new GameFiltererDescriptor(LinkPropertyId, Loc.caption_link_type())];

    public override GameFilterer? GetGameFilterer(GetGameFilterersArgs args)
        => args.ItemId == LinkPropertyId ? new LinkPropertyGameFilterer(this, args) : (GameFilterer?)null;

    public override GameGrouper? GetGameGrouper(GetGameGroupersArgs args)
        => args.ItemId == LinkPropertyId ? new LinkPropertyGrouper(this) : (GameGrouper?)null;

    public override ICollection<GameGrouperDescriptor> GetGameGrouperDescriptors(GetGameGrouperDescriptorsArgs args)
        => [new GameGrouperDescriptor(LinkPropertyId, Loc.caption_link_type())];

    public override ICollection<MenuItemDescriptor> GetGameMenuItemDescriptors(GetGameMenuItemDescriptorsArgs args)
        => MenuHandler?.GetGameMenuItemDescriptors() ?? [];

    public override ICollection<MenuItemImpl>? GetGameMenuItems(GetGameMenuItemsArgs args)
        => MenuHandler?.GetGameMenuItems(args);

    public override GameSorter? GetGameSorter(GetGameSortersArgs args)
        => args.ItemId == LinkPropertyId ? new LinkPropertySorter(this) : (GameSorter?)null;

    public override ICollection<GameSorterDescriptor> GetGameSorterDescriptors(GetGameSorterDescriptorsArgs args)
        => [new GameSorterDescriptor(LinkPropertyId, Loc.caption_link_type())];

    public override async Task<PluginSettingsHandler?> GetSettingsHandlerAsync(GetSettingsHandlerArgs args)
        => new LinkUtilitiesSettingsHandler(this);

    public override async Task InitializeAsync(InitializeArgs args)
    {
        PlayniteApi = args.Api;
        Loc.Api = args.Api;

        InstallDir = args.PluginInstallDir;
        MenuHandler = new MenuHandler(PlayniteApi);
        Plugin = this;

        await Links.InitializeAsync();

        _handleUriActions = new HandleUriActions();

        PlayniteApi.UriHandler.RegisterSource("LinkUtilities", _handleUriActions.UriHandlerAsync);

        Settings = LinkUtilitiesSettingsHandler.LoadSettings();
    }

    public override async Task OnGameCollectionChange(DataCollectionChangeArgs<Game> args)
    {
        if (!Settings.CleanUpAfterChange || IsUpdating || !args.UpdatedItems.HasItems() || PlayniteApi is null)
        {
            return;
        }

        var games = args.UpdatedItems.Where(item
            => item.OldData == null
                || (item.NewData.Links != null
                    && item.NewData.Links.Count > 0
                    && (item.OldData.Links == null
                        || !item.OldData.Links.IsListEqualExact(item.NewData.Links))))
            .Select(item => item.NewData).Distinct().Select(g => new BaseActionGame(g)).ToList();

        if (!games.HasItems())
        {
            return;
        }

        IsUpdating = true;
        await CleanUpLinks.CreateAndExecuteAsync(PlayniteApi, games, Loc.link_utilities_name(), true);
        IsUpdating = false;
    }

    public override async Task OnMetadataDownloadFinishedAsync(OnMetadataDownloadFinishedArgs args)
    {
        if (!Settings.AddLinksToNewGames || !args.Games.HasItems() || PlayniteApi is null)
        {
            IsUpdating = false;
            return;
        }

        var games = args.Games.Select(g => new BaseActionGame(g)).ToList();

        IsUpdating = true;

        if (args.StartReason == MetadataDownloadStartReason.OnNewGameImport)
        {
            await AddWebsiteLinks.CreateAndExecuteAsync(PlayniteApi, games, Loc.link_utilities_name(), false);
        }
        else
        {
            await CleanUpLinks.CreateAndExecuteAsync(PlayniteApi, games, Loc.link_utilities_name(), false);
        }

        IsUpdating = false;
    }

    public override async Task OnMetadataDownloadStartedAsync(OnMetadataDownloadStartedArgs args) => IsUpdating = true;
}