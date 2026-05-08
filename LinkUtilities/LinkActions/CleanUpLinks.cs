using Playnite;
using PlayniteExtensionHelpers.GamesCommon;

namespace LinkUtilities.LinkActions;

public class CleanUpLinks : BaseAction
{
    public override string Id => ActionIds.TypeCleanUpLinks;

    public override string Name => Loc.action_name_clean_up_links();

    public static async Task CreateAndExecuteAsync(IPlayniteApi api, List<BaseActionGame> games, string pluginName, bool showDialogs = false)
    {
        var action = new CleanUpLinks();
        var args = action.GetActionArgs(api, games, pluginName);

        args.DoForAllType = DoForAllTypes.BlockingBulkUpdate;

        args.ShowDialogs = showDialogs;

        await action.DoForAllAsync(args);
    }

    public override async Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args)
    {
        //NEXT: Add this to the actual game update event and add a menu entry for manual execution.

        var result = false;

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
            ResultMessageId = LocId.dialog_processed_links_message,
            DoForAllType = DoForAllTypes.BackgroundOperation
        };
    }
}