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

public class AddWebsiteLinksArgs(IPlayniteApi api) : BaseActionArgs(api)
{
    public AddWebsiteLinkTypes AddType { get; set; } = AddWebsiteLinkTypes.None;
}