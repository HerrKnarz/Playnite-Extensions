using LinkUtilities.DataHandlers;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.ViewModels;
using Playnite;

namespace LinkUtilities;

public class LinkUtilitiesPlugin : Plugin
{
    public const string Id = "HerrKnarz.LinkUtilities";
    public const string LinkPropertyId = "LinkUtilities.LinkPropery";
    public static string? InstallDir { get; set; }

    /// <summary>
    /// Is set to true, while the library is updated via one of the DoForAll methods. Is used to
    /// avoid endless loops caused by triggering the game update.
    /// </summary>
    public static bool IsUpdating { get; set; } = false;

    public static IPlayniteApi? PlayniteApi { get; private set; } = null!;
    public static LinkUtilitiesPlugin? Plugin { get; private set; } = null!;
    public static LinkUtilitiesPluginSettings Settings { get; set; } = new();

    public MenuHandler? MenuHandler { get; set; }

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
        => MenuHandler?.MenuItemDescriptors ?? [];

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

        await AddWebsiteLinks.Instance().Links.InitializeAsync();

        PlayniteApi.UriHandler.RegisterSource("LinkUtilities", HandleUriActions.Instance().UriHandlerAsync);

        Settings = LinkUtilitiesSettingsHandler.LoadSettings();
    }

    public override async Task OnGameCollectionChange(DataCollectionChangeArgs<Game> args)
    {
        // This is called when data in the collection are changed. args.AddedItems,
        // args.RemovedItems, args.UpdatedItems
    }
}