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

    //TODO: Check if this needs to change once themes are supported in Playnite 11.0
    private static SolidColorBrush GetIconColor() =>
            (SolidColorBrush?)System.Windows.Application.Current?.TryFindResource("TextBrush") ?? new SolidColorBrush(Colors.White);

    private ICollection<MenuItemImpl>? GetAddFromClipboardItems(List<GameEx> games)
    {
        var addLinksArgs = AddLinkFromClipboard.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());

        return [
            new(Loc.menu_add_link_from_clipboard(),
                () => AddLinkFromClipboard.Instance().DoForAllAsync(addLinksArgs),
                false,
                UIIcon.FromFontIcon("f07f", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetAddLibraryLinksItems(List<GameEx> games)
    {
        var addLibraryLinksArgs = AddLibraryLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());

        return [
            new(Loc.menu_add_library_links(),
                async () => await AddLibraryLinks.Instance().DoForAllBackgroundOrAsync(addLibraryLinksArgs),
                false,
                UIIcon.FromFontIcon("eb9c", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetAddLinksItems(List<GameEx> games)
    {
        var addLinksArgs = AddWebsiteLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());
        addLinksArgs.AddType = AddWebsiteLinkTypes.Add;

        var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                    () => AddWebsiteLinks.Instance().DoForAllBackground(addLinksArgs),
                    false,
                    UIIcon.FromFontIcon("f0c1", Playnite.Fonts.NerdFont, GetIconColor())),

                MenuItemImpl.Separator
            };

        foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
        {
            var addSingleLinksArgs = link.GetActionArgs(playniteApi, games, Loc.link_utilities_name());
            addSingleLinksArgs.AddType = AddWebsiteLinkTypes.Add;

            var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllBackgroundOrAsync(addSingleLinksArgs));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_section_add_link(), subItems)];
    }

    private ICollection<MenuItemImpl>? GetBrowserSearchItems(List<GameEx> games)
    {
        var searchBrowserArgs = AddWebsiteLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());
        searchBrowserArgs.AddType = AddWebsiteLinkTypes.SearchInBrowser;

        var subItems = new List<MenuItemImpl>();

        foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.CanBeBrowserSearched).OrderBy(x => x.LinkName))
        {
            var searchSingleBrowserArgs = link.GetActionArgs(playniteApi, games, Loc.link_utilities_name());
            searchSingleBrowserArgs.AddType = AddWebsiteLinkTypes.SearchInBrowser;

            var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllAsync(searchSingleBrowserArgs));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_search_link_in_browser(), subItems)];
    }

    private ICollection<MenuItemImpl>? GetConvertSteamlinksItems(List<GameEx> games, bool toClient = true)
    {
        var convertArgs = ConvertSteamLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());
        convertArgs.ToClient = toClient;

        return [
            new(
                toClient ? Loc.menu_convert_steam_links_to_client() : Loc.menu_convert_steam_links_to_website(),
                async () => await ConvertSteamLinks.Instance().DoForAllBackgroundOrAsync(convertArgs),
                false,
                UIIcon.FromFontIcon("f1b6", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetRemoveDuplicatesItems(List<GameEx> games)
    {
        var baseArgs = RemoveDuplicates.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());

        return [
            new(Loc.menu_remove_duplicate_links(),
                () => RemoveDuplicates.Instance().DoForAllBackground( baseArgs),
                false,
                UIIcon.FromFontIcon("f0a96", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetSearchLinksItems(List<GameEx> games)
    {
        var searchLinksArgs = AddWebsiteLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());
        searchLinksArgs.AddType = AddWebsiteLinkTypes.Search;

        var searchMissingArgs = AddWebsiteLinks.Instance().GetActionArgs(playniteApi, games, Loc.link_utilities_name());
        searchMissingArgs.AddType = AddWebsiteLinkTypes.SearchMissing;

        var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                    async () => await AddWebsiteLinks.Instance().DoForAllAsync(searchLinksArgs),
                    false,
                    UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, GetIconColor())),
                    //TODO: Change icon colors once themes are supported in Playnite 11.0

                new(Loc.menu_search_link_to_all_missing_websites(),
                    async () => await AddWebsiteLinks.Instance().DoForAllAsync(searchMissingArgs),
                    false,
                    UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, GetIconColor())),
                    //TODO: Change icon colors once themes are supported in Playnite 11.0

                MenuItemImpl.Separator
            };

        foreach (var link in AddWebsiteLinks.Instance().Links.Where(l => l.CanBeSearched && l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
        {
            var searchSingleLinksArgs = link.GetActionArgs(playniteApi, games, Loc.link_utilities_name());
            searchSingleLinksArgs.AddType = AddWebsiteLinkTypes.Search;

            var subItem = new MenuItemImpl(link.LinkName,
                async () => await link.DoForAllAsync(searchSingleLinksArgs));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_section_search_link(), subItems)];
    }
}