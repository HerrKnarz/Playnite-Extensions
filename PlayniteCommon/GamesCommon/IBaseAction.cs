namespace PlayniteCommon.GamesCommon;

/// <summary>
/// Interface for classes, that can be used as a actions to be executed for a list of games.
/// Contains texts for the progress bar, result dialog and the action to execute.
/// </summary>
public interface IBaseAction
{
    /// <summary>
    /// Message to display when executing the action.
    /// </summary>
    string ProgressMessage { get; }

    /// <summary>
    /// Message ID for the result message to display after executing the action. Must contain a
    /// placeholder for the number of affected games.
    /// </summary>
    string ResultMessageId { get; }

    /// <summary>
    /// Executes the action on all games.
    /// </summary>
    /// <param name="games">The list of games to be processed</param>
    /// <param name="args">Arguments for the game action</param>
    Task DoForAllAsync(List<GameEx> games, BaseActionArgs args);

    /// <summary>
    /// Executes the action on a game.
    /// </summary>
    /// <param name="game">The game to be processed</param>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    Task<bool> ExecuteAsync(GameEx game, BaseActionArgs args);

    /// <summary>
    /// Executes follow-up steps after the execute method was run. Should be executed after a loop
    /// containing the Execute method.
    /// </summary>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    Task FollowUpAsync(BaseActionArgs args);

    /// <summary>
    /// Prepares the link action before performing the execute method. Should be executed before a
    /// loop containing the Execute method.
    /// </summary>
    /// <param name="args">Arguments for the game action</param>
    /// <returns>true, if the action was successful</returns>
    Task<bool> PrepareAsync(BaseActionArgs args);
}