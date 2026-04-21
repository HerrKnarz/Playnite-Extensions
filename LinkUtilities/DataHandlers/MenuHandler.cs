using LinkUtilities.LinkActions;
using LinkUtilities.Models;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;
using System.Windows.Media;
using static Playnite.Plugin;

namespace LinkUtilities.DataHandlers;

public class MenuHandler(IPlayniteApi playniteApi)
{
    public const string TypeAddFromClipboard = "link.utilities.add.clipboard";
    public const string TypeAddLibraryLinks = "link.utilities.add.library.links";
    public const string TypeAddLinks = "link.utilities.add.links";
    public const string TypeBrowserSearch = "link.utilities.search.browser";
    public const string TypeConvertSteamClient = "link.utilities.convert.steam.client";
    public const string TypeConvertSteamWeb = "link.utilities.convert.steam.web";
    public const string TypeRemoveDuplicates = "link.utilities.remove.duplicates";
    public const string TypeSearchLinks = "link.utilities.search.links";

    public ICollection<MenuItemDescriptor> MenuItemDescriptors
    {
        get;
        private set;
    } =
        [
            new MenuItemDescriptor(TypeAddLinks, Loc.menu_section_add_link()),
            new MenuItemDescriptor(TypeAddLibraryLinks, Loc.menu_add_library_links()),
            new MenuItemDescriptor(TypeSearchLinks, Loc.menu_section_search_link()),
            new MenuItemDescriptor(TypeBrowserSearch, Loc.menu_search_link_in_browser()),
            new MenuItemDescriptor(TypeRemoveDuplicates, Loc.menu_remove_duplicate_links()),
            new MenuItemDescriptor(TypeAddFromClipboard, Loc.menu_add_link_from_clipboard()),
            new MenuItemDescriptor(TypeConvertSteamClient, Loc.menu_convert_steam_links_to_client()),
            new MenuItemDescriptor(TypeConvertSteamWeb, Loc.menu_convert_steam_links_to_website()),
        ];

    public ICollection<MenuItemImpl>? GetGameMenuItems(GetGameMenuItemsArgs args)
    {
        var games = args.Games.Select(g => new GameEx(g)).ToList();

        return args.ItemId switch
        {
            TypeAddFromClipboard => GetAddFromClipboardItems(games),
            TypeAddLibraryLinks => GetAddLibraryLinksItems(games),
            TypeAddLinks => GetAddLinksItems(games),
            TypeBrowserSearch => GetBrowserSearchItems(games),
            TypeConvertSteamClient => GetConvertSteamlinksItems(games),
            TypeConvertSteamWeb => GetConvertSteamlinksItems(games, false),
            TypeRemoveDuplicates => GetRemoveDuplicatesItems(games),
            TypeSearchLinks => GetSearchLinksItems(games),
            _ => throw new ArgumentOutOfRangeException(nameof(args), args.ItemId, null)
        };
    }

    private ICollection<MenuItemImpl>? GetAddFromClipboardItems(List<GameEx> games)
    {
        var addLinksArgs = new AddWebsiteLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            AddType = AddWebsiteLinkTypes.Add,
        };

        return [
            new(Loc.menu_add_link_from_clipboard(),
                    async () => await AddLinkFromClipboard.Instance().DoForAllAsync(games, addLinksArgs),
                    false,
                    UIIcon.FromFontIcon("f07f", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
    }

    private ICollection<MenuItemImpl>? GetAddLibraryLinksItems(List<GameEx> games)
    {
        var addLibraryLinksArgs = new BaseActionArgs(playniteApi, games, Loc.link_utilities_name());

        return [
            new(Loc.menu_add_library_links(),
                    async () => await AddLibraryLinks.Instance().DoForAllAsync(games, addLibraryLinksArgs),
                    false,
                    UIIcon.FromFontIcon("eb9c", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
    }

    private ICollection<MenuItemImpl>? GetAddLinksItems(List<GameEx> games)
    {
        var addLinksArgs = new AddWebsiteLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            AddType = AddWebsiteLinkTypes.Add,
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

    private ICollection<MenuItemImpl>? GetBrowserSearchItems(List<GameEx> games)
    {
        var searchBrowserArgs = new AddWebsiteLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            AddType = AddWebsiteLinkTypes.SearchInBrowser,
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

    private ICollection<MenuItemImpl>? GetConvertSteamlinksItems(List<GameEx> games, bool toClient = true)
    {
        var convertArgs = new ConvertSteamLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            ToClient = toClient,
        };

        return [
            new(
                    toClient ?
                        Loc.menu_convert_steam_links_to_client() :
                        Loc.menu_convert_steam_links_to_website(),
                    async () => await ConvertSteamLinks.Instance().DoForAllAsync(games, convertArgs),
                    false,
                    UIIcon.FromFontIcon("f1b6", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
    }

    private ICollection<MenuItemImpl>? GetRemoveDuplicatesItems(List<GameEx> games)
    {
        var baseArgs = new BaseActionArgs(playniteApi, games, Loc.link_utilities_name());

        return [
            new(Loc.menu_remove_duplicate_links(),
                    async () => await RemoveDuplicates.Instance().DoForAllAsync(games, baseArgs),
                    false,
                    UIIcon.FromFontIcon("f0a96", Playnite.Fonts.NerdFont, new SolidColorBrush(Colors.White)))];
    }

    private ICollection<MenuItemImpl>? GetSearchLinksItems(List<GameEx> games)
    {
        var searchLinksArgs = new AddWebsiteLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            AddType = AddWebsiteLinkTypes.Search,
        };

        var searchMissingArgs = new AddWebsiteLinksArgs(playniteApi, games, Loc.link_utilities_name())
        {
            AddType = AddWebsiteLinkTypes.SearchMissing,
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
}