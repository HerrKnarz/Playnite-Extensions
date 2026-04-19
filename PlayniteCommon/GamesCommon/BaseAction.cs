using Playnite;

namespace PlayniteCommon.GamesCommon;

public abstract class BaseAction : IBaseAction
{
    internal readonly List<Game> _gamesAffected = [];

    public abstract string ProgressMessage { get; }
    public abstract string ResultMessageId { get; }

    public virtual async Task DoForAllAsync(List<GameEx> games, BaseActionArgs args)
    {
        // NEXT: Set IsUpdating outside of this!
        var gamesAffected = 0;

        if (args.DebugMode)
        {
            Log.Debug($"===> Started {GetType()} for {games.Count} games. =======================");
        }

        Cursor.Current = Cursors.WaitCursor;

        try
        {
            if (!await PrepareAsync(args))
            {
                return;
            }

            if (games.Count == 1)
            {
                args.IsBulkAction = false;

                if (await ExecuteAsync(games.First(), args))
                {
                    gamesAffected++;
                }

                await FollowUpAsync(args);
            }
            // if we have more than one game in the list, we want to start buffered mode and show a
            // progress bar.
            else if (games.Count > 1)
            {
                var globalProgressOptions = new GlobalProgressOptions(
                    $"{args.PluginName} - {ProgressMessage}",
                    true
                )
                {
                    IsIndeterminate = false
                };

                await args.Api.Dialogs.ShowAsyncBlockingProgressAsync(globalProgressOptions,
                    async (globalProcessArgs) =>
                    {
                        try
                        {
                            globalProcessArgs.SetProgressMaxValue(games.Count);

                            var counter = 0;

                            // TODO: Check if using UpdateAsync with the result of old and new data could be useful.
                            foreach (var game in games)
                            {
                                globalProcessArgs.SetText($"{args.PluginName}{Environment.NewLine}{ProgressMessage}{Environment.NewLine}{game.Game?.Name}");

                                if (globalProcessArgs.CancelToken.IsCancellationRequested)
                                {
                                    break;
                                }

                                if (await ExecuteAsync(game, args))
                                {
                                    gamesAffected++;
                                }

                                globalProcessArgs.SetCrrentProgressValue(++counter);
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
                await args.Api.Dialogs.ShowMessageAsync(args.Api.GetLocalizedString(ResultMessageId, ("gameCount", gamesAffected)));
            }
        }
        finally
        {
            if (args.DebugMode)
            {
                Log.Debug($"===> Finished {GetType()} with {gamesAffected} games affected. =======================");
            }

            Cursor.Current = Cursors.Default;
        }
    }

    public abstract Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args);

    public virtual async Task FollowUpAsync(BaseActionArgs args)
    {
        if (args.GamesNeedUpdate)
        {
            await GameUpdater.UpdateGamesAsync(_gamesAffected, args.Api, args.DebugMode);
        }

        _gamesAffected.Clear();
    }

    public virtual async Task<bool> PrepareAsync(BaseActionArgs args)
    {
        _gamesAffected.Clear();

        return true;
    }
}