using Playnite;

namespace PlayniteExtensionHelpers.GamesCommon;

public class BaseActionBackgroundOp : BackgroundOperation
{
    private readonly BaseActionArgs _actionArgs;
    private readonly CancellationTokenSource _cancelToken = new();
    private readonly Func<BaseActionGame, BaseActionArgs, Task<bool>> _executeFunc;
    private readonly Func<BaseActionArgs, Task> _followUpFunc;
    private readonly Func<BaseActionArgs, Task<bool>> _prepareFunc;
    private readonly Func<Game, BaseActionGame, Task<bool>> _updateGameFunc;
    private bool _isPaused = false;
    private bool _isResumed = false;

    public BaseActionBackgroundOp(BaseActionArgs args,
            Func<BaseActionArgs, Task<bool>> prepareFunc,
        Func<BaseActionGame, BaseActionArgs, Task<bool>> executeFunc,
        Func<BaseActionArgs, Task> followUpFunc,
        Func<Game, BaseActionGame, Task<bool>> updateGameFunc) : base(args.Id, $"{args.PluginName}: {args.Name}")
    {
        Pausable = true;
        _actionArgs = args;
        _prepareFunc = prepareFunc;
        _executeFunc = executeFunc;
        _followUpFunc = followUpFunc;
        _updateGameFunc = updateGameFunc;
    }

    public virtual async Task BackgroundUpdateAsync()
    {
        if (_actionArgs.GamesNeedUpdate)
        {
            await _actionArgs.Api.Library.Games.UpdateAsync([.. _actionArgs.Games.Where(g => g.NeedsToBeUpdated).Select(g => g.Game.Id)], async (game) => await UpdateInDbInFollowUpAsync(game));
        }
    }

    public override async ValueTask DisposeAsync()
    {
        _cancelToken.Dispose();
        GC.SuppressFinalize(this);
    }

    public override async Task PauseAsync(PauseArgs args) => _isPaused = true;

    public override async Task ResumeAsync(ResumeArgs args)
    {
        _isPaused = false;
        _isResumed = true;

        await StartAsync(new StartArgs());
    }

    public override async Task StartAsync(StartArgs args)
    {
        _ = Task.Run(async () =>
        {
            try
            {
                try
                {
                    if (_actionArgs.DebugMode)
                    {
                        Log.Debug($"===> {(_isPaused ? "Resumed" : "Started")} {_actionArgs.Id} for {_actionArgs.Games.Count} games. =======================");
                    }

                    var counter = 0;

                    Status = _actionArgs.ProgressMessage;
                    ProgressIsIndeterminate = false;
                    ProgressMaximum = _actionArgs.Games.Count;

                    if (!_isResumed)
                    {
                        if (!await _prepareFunc(_actionArgs))
                        {
                            return;
                        }
                    }
                    else
                    {
                        counter = (int)ProgressValue;
                    }

                    foreach (var game in _actionArgs.Games.Where(g => !g.Processed).ToList())
                    {
                        if (_cancelToken.IsCancellationRequested || _isPaused)
                        {
                            break;
                        }

                        game.Processed = true;

                        Status = $"{_actionArgs.ProgressMessage}{Environment.NewLine}{game.Game?.Name}";

                        game.NeedsToBeUpdated = game.Game is not null && await _executeFunc(game, _actionArgs);

                        ProgressValue = ++counter;
                    }

                    if (_isPaused)
                    {
                        return;
                    }

                    await BackgroundUpdateAsync();

                    await _followUpFunc(_actionArgs);

                    Status = _actionArgs.Api.GetLocalizedString(_actionArgs.ResultMessageId, ("gameCount", _actionArgs.Games.Count(g => g.NeedsToBeUpdated)));

                    await OperationFinishedAsync(new FinishedEventArgs());
                }
                catch (Exception e)
                {
                    Log.Error(e);
                    await OperationFailedAsync(new FailedEventArgs(e.Message));
                }
            }
            finally
            {
                if (_actionArgs.DebugMode)
                {
                    Log.Debug($"===> {(_isPaused ? "Paused" : "Finished")} {_actionArgs.Id} with {_actionArgs.Games.Count(g => g.NeedsToBeUpdated)} games affected. =======================");
                }
            }
        });
    }

    public override async Task StopAsync(StopArgs args) => await _cancelToken.CancelAsync();

    /// <summary>
    /// Updates the game directly in the database after it was processed in the loop. Should only be
    /// called in an UpdateAsync method of the game library. The game will only be updated, if the
    /// method returns true.
    /// </summary>
    /// <param name="game">Game to update</param>
    /// <returns>true, if the game needs to be updated.</returns>
    public virtual async Task<bool> UpdateInDbInFollowUpAsync(Game game)
    {
        var processedGame = _actionArgs?.Games.FirstOrDefault(g => g.GameId.Equals(game.Id));

        return processedGame is not null && await _updateGameFunc(game, processedGame);
    }
}