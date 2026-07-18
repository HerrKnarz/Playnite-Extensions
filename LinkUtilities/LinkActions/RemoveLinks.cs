using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class RemoveLinks : BaseAction
{
    public override string Id => ActionIds.TypeRemoveLinks;

    public override string Name => Loc.action_name_remove_links();

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        var action = new RemoveLinks();
        await action.DoForAllAsync(action.GetActionArgs(api, games, pluginName));
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
        => RemoveUnwantedLinks(game.Game);

    public override BaseActionArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new BaseActionArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_removing_links(),
            ResultMessageId = LocId.dialog_removed_links_message
        };
    }

    /// <summary>
    /// Removes unwanted links from a game
    /// </summary>
    /// <param name="game">Game in which the unwanted links will be removed.</param>
    /// <returns>
    /// True, if unwanted links were removed. Returns false if there weren't any to begin with.
    /// </returns>
    private static bool RemoveUnwantedLinks(Game game)
    {
        if (!game.Links.HasItems())
        {
            return false;
        }

        var result = false;

        foreach (var link in game.Links.ToList())
        {
            var linkTypeIdentifier = link.TypeId;
            var tempLinkName = string.Empty;

            if (LinkUtilitiesPlugin.Settings.RemovePatterns.LinkMatch(ref tempLinkName, link.Url ?? string.Empty, ref linkTypeIdentifier, Models.PatternMatchModes.MatchByUrlAndType))
            {
                result |= game.Links.Remove(link);
            }
        }

        return result;
    }
}

//NEXT: Add rename functionality