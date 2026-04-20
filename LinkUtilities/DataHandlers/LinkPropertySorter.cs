using LinkUtilities.Helper;
using Playnite;

namespace LinkUtilities.DataHandlers;

internal class LinkPropertySorter(LinkUtilitiesPlugin plugin) : GameSorter([BuiltInGameDataId.Links])
{
    private readonly LinkUtilitiesPlugin plugin = plugin;

    public override void BeginSort(BeginSortArgs args)
    {
    }

    public override int CompareForSort(CompareArgs args) =>
        LinkHelper.LinkNames(args.GameA).CompareTo(LinkHelper.LinkNames(args.GameB), StringComparison.Ordinal);

    public override void EndSort(EndSortArgs args)
    {
    }
}