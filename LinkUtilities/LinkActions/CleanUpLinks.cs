using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class CleanUpLinks : BaseAction
{
    public override string Id => ActionIds.TypeCleanUpLinks;

    public override string Name => Loc.action_name_clean_up_links();

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName, bool showDialog = false)
    {
        var action = new CleanUpLinks();
        var args = action.GetActionArgs(api, games, pluginName);

        args.ShowDialog = showDialog;

        await action.DoForAllAsync(args);
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        var result = false;

        if (LinkUtilitiesPlugin.Settings.RemoveUnwantedAfterChange)
        {
            var action = new RemoveLinks();
            var removeArgs = action.GetActionArgs(args.Api, args.Games, args.PluginName);

            result |= await action.ExecuteAsync(game, removeArgs);
        }

        if (LinkUtilitiesPlugin.Settings.ConvertSteamLinksAfterChange)
        {
            var action = new ConvertSteamLinks();
            var steamArgs = action.GetActionArgs(args.Api, args.Games, args.PluginName);
            steamArgs.ToClient = true;

            result |= await action.ExecuteAsync(game, steamArgs);
        }

        if (LinkUtilitiesPlugin.Settings.RemoveDuplicatesAfterChange)
        {
            var action = new RemoveDuplicates();
            var removeArgs = action.GetActionArgs(args.Api, args.Games, args.PluginName);

            result |= await action.ExecuteAsync(game, removeArgs);
        }

        return result;
    }

    public override BaseActionArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName)
    {
        return new BaseActionArgs(Id, Name, api, games, pluginName)
        {
            ProgressMessage = Loc.progress_processing_links(),
            ResultMessageId = LocId.dialog_processed_links_message
        };
    }
}