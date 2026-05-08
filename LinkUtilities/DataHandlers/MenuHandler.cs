using LinkUtilities.LinkActions;
using LinkUtilities.Linker;
using Playnite;
using PlayniteExtensionHelpers.GamesCommon;
using System.Windows.Media;
using static Playnite.Plugin;

namespace LinkUtilities.DataHandlers;

public class MenuHandler(IPlayniteApi playniteApi)
{
    public ICollection<MenuItemDescriptor> MenuItemDescriptors
    {
        get;
        private set;
    } =
        [
            new MenuItemDescriptor(ActionIds.TypeAddLinks, Loc.menu_section_add_link()),
            new MenuItemDescriptor(ActionIds.TypeAddLibraryLinks, Loc.menu_add_library_links()),
            new MenuItemDescriptor(ActionIds.TypeSearchLinks, Loc.menu_section_search_link()),
            new MenuItemDescriptor(ActionIds.TypeBrowserSearch, Loc.menu_search_link_in_browser()),
            new MenuItemDescriptor(ActionIds.TypeRemoveDuplicates, Loc.menu_remove_duplicate_links()),
            new MenuItemDescriptor(ActionIds.TypeCleanUpLinks, Loc.menu_clean_up_links()),
            new MenuItemDescriptor(ActionIds.TypeAddFromClipboard, Loc.menu_add_link_from_clipboard()),
            new MenuItemDescriptor(ActionIds.TypeConvertSteamClient, Loc.menu_convert_steam_links_to_client()),
            new MenuItemDescriptor(ActionIds.TypeConvertSteamWeb, Loc.menu_convert_steam_links_to_website()),
            new MenuItemDescriptor(ActionIds.TypeTestAdd, "Test Add Links"),
        ];

    public ICollection<MenuItemImpl>? GetGameMenuItems(GetGameMenuItemsArgs args)
    {
        var games = args.Games.Select(g => new BaseActionGame(g)).ToList();

        return args.ItemId switch
        {
            ActionIds.TypeAddFromClipboard => GetAddFromClipboardItems(games),
            ActionIds.TypeAddLibraryLinks => GetAddLibraryLinksItems(games),
            ActionIds.TypeAddLinks => GetAddLinksItems(games),
            ActionIds.TypeBrowserSearch => GetBrowserSearchItems(games),
            ActionIds.TypeCleanUpLinks => GetCleanUpLinksItems(games),
            ActionIds.TypeConvertSteamClient => GetConvertSteamlinksItems(games),
            ActionIds.TypeConvertSteamWeb => GetConvertSteamlinksItems(games, false),
            ActionIds.TypeRemoveDuplicates => GetRemoveDuplicatesItems(games),
            ActionIds.TypeSearchLinks => GetSearchLinksItems(games),
            ActionIds.TypeTestAdd => GetTestAddItems(games),
            _ => throw new ArgumentOutOfRangeException(nameof(args), args.ItemId, null)
        };
    }

    //TODO: Check if this needs to change once themes are supported in Playnite 11.0
    private static SolidColorBrush GetIconColor() =>
            (SolidColorBrush?)System.Windows.Application.Current?.TryFindResource("TextBrush") ?? new SolidColorBrush(Colors.White);

    private ICollection<MenuItemImpl>? GetAddFromClipboardItems(List<BaseActionGame> games)
    {
        return [
            new(Loc.menu_add_link_from_clipboard(),
                (clickArgs) => AddLinkFromClipboard.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name()),
                false,
                UIIcon.FromFontIcon("f07f", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetAddLibraryLinksItems(List<BaseActionGame> games)
    {
        return [
            new(Loc.menu_add_library_links(),
                async (clickArgs) => await AddLibraryLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name()),
                false,
                UIIcon.FromFontIcon("eb9c", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetAddLinksItems(List<BaseActionGame> games)
    {
        if (LinkUtilitiesPlugin.Plugin is null)
        {
            return null;
        }

        var linkDict = new LinkDict();

        var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                    async (clickArgs) => await AddWebsiteLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name(), false),
                    false,
                    UIIcon.FromFontIcon("f0c1", Playnite.Fonts.NerdFont, GetIconColor())),

                MenuItemImpl.Separator
            };

        foreach (var link in LinkUtilitiesPlugin.Plugin.Links.Where(l => l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
        {
            var subItem = new MenuItemImpl(link.LinkName,
                async (clickArgs) => await linkDict.CreateAndExecuteAsync(link.Id, playniteApi, games, Loc.link_utilities_name(), LinkActionType.Add));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_section_add_link(), subItems)];
    }

    private ICollection<MenuItemImpl>? GetBrowserSearchItems(List<BaseActionGame> games)
    {
        if (LinkUtilitiesPlugin.Plugin is null)
        {
            return null;
        }

        var linkDict = new LinkDict();

        var subItems = new List<MenuItemImpl>();

        foreach (var link in LinkUtilitiesPlugin.Plugin.Links.Where(l => l.CanBeBrowserSearched).OrderBy(x => x.LinkName))
        {
            var subItem = new MenuItemImpl(link.LinkName,
                async (clickArgs) => await linkDict.CreateAndExecuteAsync(link.Id, playniteApi, games, Loc.link_utilities_name(), LinkActionType.BrowserSearch));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_search_link_in_browser(), subItems)];
    }

    private ICollection<MenuItemImpl>? GetCleanUpLinksItems(List<BaseActionGame> games)
    {
        return [
            new(Loc.menu_clean_up_links(),
                (clickArgs) => CleanUpLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name(), true),
                false,
                UIIcon.FromFontIcon("f19a1", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetConvertSteamlinksItems(List<BaseActionGame> games, bool toClient = true)
    {
        return [
            new(
                toClient ? Loc.menu_convert_steam_links_to_client() : Loc.menu_convert_steam_links_to_website(),
                async (clickArgs) => await ConvertSteamLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name(), toClient),
                false,
                UIIcon.FromFontIcon("f1b6", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private ICollection<MenuItemImpl>? GetRemoveDuplicatesItems(List<BaseActionGame> games)
    {
        return [
            new(Loc.menu_remove_duplicate_links(),
                (clickArgs) => RemoveDuplicates.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name()),
                false,
                UIIcon.FromFontIcon("f0a96", Playnite.Fonts.NerdFont, GetIconColor()))];
    }

    private List<MenuItemImpl>? GetSearchLinksItems(List<BaseActionGame> games)
    {
        var subItems = new List<MenuItemImpl>
            {
                new(Loc.menu_add_link_to_all_enabled_websites(),
                    async (clickArgs) => await SearchWebsiteLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name(), false),
                    false,
                    UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, GetIconColor())),

                new(Loc.menu_search_link_to_all_missing_websites(),
                    async (clickArgs) => await SearchWebsiteLinks.CreateAndExecuteAsync(playniteApi, games, Loc.link_utilities_name(), true),
                    false,
                    UIIcon.FromFontIcon("f002", Playnite.Fonts.NerdFont, GetIconColor())),

                MenuItemImpl.Separator
            };

        if (LinkUtilitiesPlugin.Plugin is null)
        {
            return subItems;
        }

        var linkDict = new LinkDict();

        foreach (var link in LinkUtilitiesPlugin.Plugin.Links.Where(l => l.CanBeSearched && l.Settings.ShowInMenus).OrderBy(x => x.LinkName))
        {
            var subItem = new MenuItemImpl(link.LinkName,
                async (clickArgs) => await linkDict.CreateAndExecuteAsync(link.Id, playniteApi, games, Loc.link_utilities_name(), LinkActionType.Search));

            subItems.Add(subItem);
        }

        return [new MenuItemImpl(Loc.menu_section_search_link(), subItems)];
    }

    private List<MenuItemImpl>? GetTestAddItems(List<BaseActionGame> games)
    {
        return [
            new("Test Add Link",
                (clickArgs) => AddWebsiteLinks.CreateAndTestAsync(playniteApi, Loc.link_utilities_name()),
                false,
                UIIcon.FromFontIcon("f0c1", Playnite.Fonts.NerdFont, GetIconColor()))];
    }
}