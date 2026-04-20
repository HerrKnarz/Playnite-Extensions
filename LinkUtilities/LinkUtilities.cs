using LinkUtilities.DataHandlers;
using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using LinkUtilities.ViewModels;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;
using System.Windows.Media;

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

    public override GameExplorer? GetGameExplorer(GetGameExplorersArgs args) => args.ItemId == LinkPropertyId ? new LinkPropertyGameExplorer() : (GameExplorer?)null;

    public override ICollection<GameExplorerDescriptor> GetGameExplorerDescriptors(GetGameExplorerDescriptorsArgs args)
    {
        return
        [
            new GameExplorerDescriptor(LinkPropertyId, Loc.caption_link_type())
        ];
    }

    public override ICollection<GameFiltererDescriptor> GetGameFilterDescriptors(GetGameFiltereDescriptorsArgs args)
    {
        return
        [
            new GameFiltererDescriptor(LinkPropertyId, Loc.caption_link_type())
        ];
    }

    public override GameFilterer? GetGameFilterer(GetGameFilterersArgs args) => args.ItemId == LinkPropertyId ? new LinkPropertyGameFilterer(this, args) : (GameFilterer?)null;

    public override GameGrouper? GetGameGrouper(GetGameGroupersArgs args) => args.ItemId == LinkPropertyId ? new LinkPropertyGrouper(this) : (GameGrouper?)null;

    public override ICollection<GameGrouperDescriptor> GetGameGrouperDescriptors(GetGameGrouperDescriptorsArgs args)
    {
        return
        [
            new GameGrouperDescriptor(LinkPropertyId, Loc.caption_link_type())
        ];
    }

    public override ICollection<MenuItemDescriptor> GetGameMenuItemDescriptors(GetGameMenuItemDescriptorsArgs args)
    {
        return
        [
            new MenuItemDescriptor("link.utilities.add.links", Loc.menu_section_add_link()),
            new MenuItemDescriptor("link.utilities.add.library.links", Loc.menu_add_library_links()),
            new MenuItemDescriptor("link.utilities.search.links", Loc.menu_section_search_link()),
            new MenuItemDescriptor("link.utilities.search.browser", Loc.menu_search_link_in_browser()),
            new MenuItemDescriptor("link.utilities.remove.duplicates", Loc.menu_remove_duplicate_links()),
            new MenuItemDescriptor("link.utilities.add.clipboard", Loc.menu_add_link_from_clipboard()),
            new MenuItemDescriptor("link.utilities.convert.steam.client", Loc.menu_convert_steam_links_to_client()),
            new MenuItemDescriptor("link.utilities.convert.steam.web", Loc.menu_convert_steam_links_to_website()),
        ];
    }

    public override ICollection<MenuItemImpl>? GetGameMenuItems(GetGameMenuItemsArgs args)
    {
        if (PlayniteApi is null)
        {
            return null;
        }

        var games = args.Games.Select(g => new GameEx(g)).ToList();

        if (args.ItemId == "link.utilities.add.links")
        {
            var addLinksArgs = new AddWebsiteLinksArgs(PlayniteApi)
            {
                AddType = AddWebsiteLinkTypes.Add,
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                async () => await AddWebsiteLinks.Instance().DoForAllAsync(games, addLinksArgs),
                false,
                UIIcon.FromFontIcon("f0c1", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White))),
                //TODO: Change icon colors once themes are supported in Playnite 11.0

                MenuItemImpl.Separator
            };

            foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
            {
                var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllAsync(games, addLinksArgs));

                subItems.Add(subItem);
            }

            return [new MenuItemImpl(Loc.menu_section_add_link(), subItems)];
        }

        if (args.ItemId == "link.utilities.add.library.links"
            && games.Any(g => AddLibraryLinks.Instance().LibraryLinks.ContainsKey(g.Game.LibraryId)))
        {
            var addLibraryLinksArgs = new BaseActionArgs(PlayniteApi)
            {
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            return [
                new(Loc.menu_add_library_links(),
                    async () => await AddLibraryLinks.Instance().DoForAllAsync(games, addLibraryLinksArgs),
                    false,
                    UIIcon.FromFontIcon("eb9c", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
        }

        if (args.ItemId == "link.utilities.search.links")
        {
            var searchLinksArgs = new AddWebsiteLinksArgs(PlayniteApi)
            {
                AddType = AddWebsiteLinkTypes.Search,
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            var searchMissingArgs = new AddWebsiteLinksArgs(PlayniteApi)
            {
                AddType = AddWebsiteLinkTypes.SearchMissing,
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                async () => await AddWebsiteLinks.Instance().DoForAllAsync(games, searchLinksArgs),
                false,
                UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White))),
                //TODO: Change icon colors once themes are supported in Playnite 11.0

                new(Loc.menu_search_link_to_all_missing_websites(),
                async () => await AddWebsiteLinks.Instance().DoForAllAsync(games, searchMissingArgs),
                false,
                UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White))),
                //TODO: Change icon colors once themes are supported in Playnite 11.0

                MenuItemImpl.Separator
            };

            foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.CanBeSearched && l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
            {
                var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllAsync(games, searchLinksArgs));

                subItems.Add(subItem);
            }

            return [new MenuItemImpl(Loc.menu_section_search_link(), subItems)];
        }

        if (args.ItemId == "link.utilities.search.browser")
        {
            var searchBrowserArgs = new AddWebsiteLinksArgs(PlayniteApi)
            {
                AddType = AddWebsiteLinkTypes.SearchInBrowser,
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            var subItems = new List<MenuItemImpl>();

            foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.CanBeBrowserSearched).OrderBy(x => x.LinkName))
            {
                var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllAsync(games, searchBrowserArgs));

                subItems.Add(subItem);
            }

            return [new MenuItemImpl(Loc.menu_search_link_in_browser(), subItems)];
        }

        if (args.ItemId == "link.utilities.remove.duplicates")
        {
            var baseArgs = new BaseActionArgs(PlayniteApi)
            {
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            return [
                new(Loc.menu_remove_duplicate_links(),
                    async () => await RemoveDuplicates.Instance().DoForAllAsync(games, baseArgs),
                    false,
                    UIIcon.FromFontIcon("f0a96", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
        }

        if (args.ItemId == "link.utilities.add.clipboard")
        {
            var addLinksArgs = new AddWebsiteLinksArgs(PlayniteApi)
            {
                AddType = AddWebsiteLinkTypes.Add,
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            return [
                new(Loc.menu_add_link_from_clipboard(),
                    async () => await AddLinkFromClipboard.Instance().DoForAllAsync(games, addLinksArgs),
                    false,
                    UIIcon.FromFontIcon("f07f", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
        }

        if (args.ItemId.StartsWith("link.utilities.convert.steam."))
        {
            var convertArgs = new ConvertSteamLinksArgs(PlayniteApi)
            {
                ToClient = args.ItemId == "link.utilities.convert.steam.client",
                IsBulkAction = args.Games.Count > 1,
                PluginName = Loc.link_utilities_name()
            };

            return [
                new(
                    args.ItemId == "link.utilities.convert.steam.client" ?
                        Loc.menu_convert_steam_links_to_client() :
                        Loc.menu_convert_steam_links_to_website(),
                    async () => await ConvertSteamLinks.Instance().DoForAllAsync(games, convertArgs),
                    false,
                    UIIcon.FromFontIcon("f1b6", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
        }

        return null;
    }

    public override GameSorter? GetGameSorter(GetGameSortersArgs args) => args.ItemId == LinkPropertyId ? new LinkPropertySorter(this) : (GameSorter?)null;

    public override ICollection<GameSorterDescriptor> GetGameSorterDescriptors(GetGameSorterDescriptorsArgs args)
    {
        return
        [
            new GameSorterDescriptor(LinkPropertyId, Loc.caption_link_type())
        ];
    }

    public override async Task<PluginSettingsHandler?> GetSettingsHandlerAsync(GetSettingsHandlerArgs args)
    {
        await Task.CompletedTask;
        return new LinkUtilitiesSettingsHandler(this);
    }

    public override async Task InitializeAsync(InitializeArgs args)
    {
        PlayniteApi = args.Api;
        Loc.Api = args.Api;
        InstallDir = args.PluginInstallDir;
        Plugin = this;

        await AddWebsiteLinks.Instance().Links.InitializeAsync();

        PlayniteApi.UriHandler.RegisterSource("LinkUtilities", HandleUriActions.Instance().UriHandlerAsync);

        Settings = LinkUtilitiesSettingsHandler.LoadSettings();
    }
}