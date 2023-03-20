using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    public enum ActionModifierTypes { None, Add, Search, Name, SortOrder }

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
        /// instance of the extension to access settings etc.
        /// </summary>
        LinkUtilities Plugin { get; }

        /// <summary>
        /// Executes the action on a game.
        /// </summary>
        /// <param name="game">The game to be processed</param>
        /// <param name="actionModifier">Optional modifier for the underlying class to do different things in the execute method</param>
        /// <param name="isBulkAction">If true the action is executed for more than one game in a loop. Can be used to do things 
        /// differently if only one game is processed.</param>
        /// <returns>true, if the action was successful</returns>
        bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true);
    }
}
