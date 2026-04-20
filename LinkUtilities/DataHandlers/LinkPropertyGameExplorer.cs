using Playnite;

namespace LinkUtilities.DataHandlers;

internal class LinkPropertyGameExplorer : GameExplorer
{
    public override List<GameExplorerItem> GetExplorableItems(GetExplorableItemsArgs args)
    {
        return LinkUtilitiesPlugin.PlayniteApi is null
            ? []
            : [.. LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes.OrderBy(l => l.Name).Select(l => new GameExplorerItem(l.Id, l.Name, new GameExplorerFilterData(LinkUtilitiesPlugin.LinkPropertyId, l.Id)))];
    }
}