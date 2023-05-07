using Playnite.SDK.Models;

namespace LinkUtilities
{
    public enum ActionModifierTypes
    {
        None,
        Add,
        Search,
        SearchMissing,
        Name,
        SortOrder,
        DontRename
    }

    /// <summary>
    /// Interface for classes, that can be used as a link action. Contains texts for the progress bar, result dialog and the action to
    /// execute.
    /// </summary>
    internal interface ILinkAction
    {
        /// <summary>
        /// Resource for the localized text in the progress bar
        /// </summary>
        string ProgressMessage { get; }

        /// <summary>
        /// Resource for the localized text in the result dialog. Should contain placeholder for the number of affected games.
        /// </summary>
        string ResultMessage { get; }

        /// <summary>
        /// Prepares the link action before performing the execute method. Should be executed before a loop containing the Execute method.
        /// </summary>
        /// <param name="actionModifier">Optional modifier for the underlying class to do different things in the execute method</param>
        /// <param name="isBulkAction">If true the action is executed for more than one game in a loop. Can be used to do things
        /// differently if only one game is processed.</param>
        /// <returns>true, if the action was successful</returns>
        bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true);

        /// <summary>
        /// Executes the action on a game.
        /// </summary>
        /// <param name="game">The game to be processed</param>
        /// <param name="actionModifier">Optional modifier for the underlying class to do different things in the execute method</param>
        /// <param name="isBulkAction">If true the action is executed for more than one game in a loop. Can be used to do things
        /// differently if only one game is processed. If false, the Prepare method will also be executed here!</param>
        /// <returns>true, if the action was successful</returns>
        bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true);
    }
}