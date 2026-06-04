using Playnite;

namespace PlayniteExtensionHelpers.GamesCommon;

/// <summary>
/// Base class to be used to execute an action for a list of games. Needs to be inherited by the
/// actual action class.
/// </summary>
public abstract class BaseAction
{
    public virtual string Id { get; } = "base.action";
    public virtual string Name { get; } = "Base Action";

    /// <summary>
    /// Executes the action on all games in a blocking Task.
    /// </summary>
    /// <param name="args">Arguments for the game action</param>
    public virtual async Task DoForAllAsync(BaseActionArgs args)
    {
        switch (args.DoForAllType)
        {
            case DoForAllTypes.BlockingLoop:
                await RunInBlockingLoop(args);
                break;

            // TODO: If possible check if the same operation is still running and ask if the user still wants to add another one to the list.
            case DoForAllTypes.BackgroundOperation:
                args.Api.AddBackgroundOperation(new BaseActionBackgroundOp(args, PrepareAsync, ExecuteAsync, FollowUpAsync, ProcessUpdateDataAsync));
                break;

            case DoForAllTypes.SingleBlockingMultiBackground:
                if (args.Games.Count == 1)
                {
                    await RunInBlockingLoop(args);
                }
                else
                {
                    args.Api.AddBackgroundOperation(new BaseActionBackgroundOp(args, PrepareAsync, ExecuteAsync, FollowUpAsync, ProcessUpdateDataAsync));
                }

                break;

            default: break;
        }
    }

    /// <summary>
    /// Executes the action on a game.
    /// </summary>
    /// <param name="game">The game to be processed</param>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    public abstract Task<bool> ExecuteAsync(BaseActionGame game, BaseActionArgs args);

    /// <summary>
    /// Executes follow-up steps after the execute method was run. Should be executed after a loop
    /// containing the Execute method.
    /// </summary>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    public virtual async Task FollowUpAsync(BaseActionArgs args)
    { }

    /// <summary>
    /// Creates an instance of the arguments needed to perform the action
    /// </summary>
    /// <param name="api">Instance of the playnite API</param>
    /// <param name="games">List of games the action will be executed for</param>
    /// <param name="pluginName">Name of the plugin</param>
    /// <returns>arguments to use in the action</returns>
    public virtual BaseActionArgs GetActionArgs(IPlayniteApi api, List<BaseActionGame> games, string pluginName) => new(Id, Name, api, games, pluginName);

    /// <summary>
    /// Prepares the link action before performing the execute method. Should be executed before a
    /// loop containing the Execute method.
    /// </summary>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    public virtual async Task<bool> PrepareAsync(BaseActionArgs args) => true;

    /// <summary>
    /// Update the database record for the specified game using values from the processed game.
    /// </summary>
    /// <remarks>
    /// This method gets called in UpdateInDb, where it will result in an actual update of the game
    /// in the library. It should only update the fields in the game, but not call UpdateAsync itself.
    /// </remarks>
    /// <param name="gameToUpdate">The game entity to update in the database.</param>
    /// <param name="processedGame">
    /// The processed game containing values to apply to the database record.
    /// </param>
    /// <returns>True if the update was applied; otherwise, false.</returns>
    public virtual async Task<bool> ProcessUpdateDataAsync(Game gameToUpdate, BaseActionGame processedGame) => false;

    public virtual async Task RunInBlockingLoop(BaseActionArgs args)
    {
        if (args.DebugMode)
        {
            Log.Debug($"===> Started {args.Name} for {args.Games.Count} games. =======================");
        }

        Cursor.Current = Cursors.WaitCursor;

        try
        {
            try
            {
                if (!await PrepareAsync(args))
                {
                    return;
                }

                var globalProgressOptions = new GlobalProgressOptions(
                    $"{args.PluginName} - {args.ProgressMessage}",
                    true
                )
                {
                    IsIndeterminate = false
                };

                await args.Api.Dialogs.ShowAsyncBlockingProgressAsync(globalProgressOptions,
                    async (globalProgressArgs) =>
                    {
                        try
                        {
                            globalProgressArgs.SetProgressMaxValue(args.Games.Count);

                            var counter = 0;

                            async Task ExecuteActionAsync(Game game)
                            {
                                globalProgressArgs.SetText($"{args.PluginName}{Environment.NewLine}{args.ProgressMessage}{Environment.NewLine}{game.Name}");

                                if (globalProgressArgs.CancelToken.IsCancellationRequested)
                                {
                                    return;
                                }

                                var baseGame = args.Games.FirstOrDefault(b => b.GameId == game.Id);

                                if (baseGame is null)
                                {
                                    return;
                                }

                                baseGame.Game = game;
                                baseGame.Processed = true;
                                baseGame.NeedsToBeUpdated = await ExecuteAsync(baseGame, args);

                                globalProgressArgs.SetCurrentProgressValue(++counter);
                            }

                            if (args.UpdateGamesAfterLoop)
                            {
                                foreach (var game in args.Games)
                                {
                                    await ExecuteActionAsync(game.Game);
                                }

                                if (args.GamesNeedUpdate)
                                {
                                    await args.Api.Library.Games.UpdateAsync([.. args.Games.Where(g => g.NeedsToBeUpdated).Select(g => g.Game.Id)], async (game) =>
                                    {
                                        var processedGame = args?.Games.FirstOrDefault(g => g.GameId.Equals(game.Id));

                                        if (processedGame is not null)
                                        {
                                            await ProcessUpdateDataAsync(game, processedGame);
                                        }
                                    });
                                }
                            }
                            else
                            {
                                await UIDispatcher.InvokeAsync(async delegate
                                {
                                    await args.Api.Library.Games.UpdateAsync(
                                        [.. args.Games.Where(g => !g.Processed).Select(g => g.Game.Id)],
                                        async (g) => await ExecuteActionAsync(g));
                                });
                            }

                            await FollowUpAsync(args);
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex);
                        }
                    });

                if (!args.ShowDialogs)
                {
                    return;
                }

                Cursor.Current = Cursors.Default;
                await args.Api.Dialogs.ShowMessageAsync(args.Api.GetLocalizedString(args.ResultMessageId, ("gameCount", args.Games.Count(g => g.NeedsToBeUpdated))));
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
        finally
        {
            if (args.DebugMode)
            {
                Log.Debug($"===> Finished {args.Name} with {args.Games.Count(g => g.NeedsToBeUpdated)} games affected. =======================");
            }

            Cursor.Current = Cursors.Default;
        }
    }
}