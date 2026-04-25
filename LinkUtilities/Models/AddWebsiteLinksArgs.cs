using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.Models;

public enum AddWebsiteLinkTypes
{
    None,
    Add,
    AddSelected,
    Search,
    SearchMissing,
    SearchSelected,
    SearchInBrowser,
}

public class AddWebsiteLinksArgs(string id, string name, IPlayniteApi api, List<BaseActionGame> games, string pluginName) : BaseActionArgs(id, name, api, games, pluginName)
{
    public AddWebsiteLinkTypes AddType { get; set; } = AddWebsiteLinkTypes.None;
}