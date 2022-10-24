using Playnite.SDK.Models;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Interface for classes, that can be used as a link action. Contains texts for the progress bar, result dialog and the action to
    /// execute.
    /// </summary>
    public interface ILinkAction
    {
        /// <summary>
        /// Ressource for the localized text in the progress bar
        /// </summary>
        string ProgressMessage { get; }
        /// <summary>
        /// Ressource for the localized text in the result dialog. Should contain placeholder for the number of affected games.
        /// </summary>
        string ResultMessage { get; }
        /// <summary>
        /// Settings to use for the action
        /// </summary>
        LinkUtilitiesSettings Settings { get; set; }

        /// <summary>
        /// Executes the action on a game.
        /// </summary>
        /// <param name="game">The game to be processed</param>
        /// <param name="actionModifier">Optional modifier for the underlying class to do different things in the execute method</param>
        /// <returns>true, if the action was successful</returns>
        bool Execute(Game game, string actionModifier = "");
    }
}
