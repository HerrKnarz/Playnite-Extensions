using Playnite;

namespace LinkUtilities.DataHandlers;

public class LinkPropertyGrouper(LinkUtilitiesPlugin plugin) : GameGrouper([BuiltInGameDataId.Links])
{
    private readonly LinkUtilitiesPlugin plugin = plugin;

    public override void BeginGrouping(BeginGroupingArgs args)
    {
    }

    public override int CompareGroups(CompareGroupsArgs args) =>
        args.GroupA.Name.CompareTo(args.GroupB.Name, StringComparison.Ordinal);

    public override void EndGrouping(EndGroupingArgs args)
    {
    }

    public override List<GameGroup>? GetGroups(GetGroupsArgs args)
    {
        if (LinkUtilitiesPlugin.PlayniteApi is null)
        {
            return null;
        }

        var groups = new List<GameGroup>();

        foreach (var link in args.Game.Links ?? [])
        {
            if (link.TypeId is null)
            {
                continue;
            }

            var group = new GameGroup(link.TypeId, LinkUtilitiesPlugin.PlayniteApi.Library.WebLinkTypes.First(t => t.Id.Equals(link.TypeId)).Name);

            groups.Add(group);
        }

        if (groups.Count == 0)
        {
            groups.Add(new GameGroup("None", "None"));
        }

        return groups;
    }
}