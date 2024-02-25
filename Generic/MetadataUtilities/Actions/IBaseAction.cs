using Playnite.SDK.Models;

namespace MetadataUtilities.Actions
{
    internal interface IBaseAction
    {
        /// <summary>
        /// Resource for the localized text in the progress bar
        /// </summary>
        string ProgressMessage { get; }

        /// <summary>
        /// Resource for the localized text in the result dialog. Should contain placeholder for the
        /// number of affected games.
        /// </summary>
        string ResultMessage { get; }

        /// <summary>
        /// Executes the action on a game.
        /// </summary>
        /// <param name="game">The game to be processed</param>
        /// <param name="actionModifier">
        /// Optional modifier for the underlying class to do different things in the execute method
        /// </param>
        /// <param name="item">The item to be processed. The type is defined by the actionModifier.</param>
        /// <param name="isBulkAction">
        /// If true the action is executed for more than one game in a loop. Can be used to do
        /// things differently if only one game is processed. If false, the Prepare method will also
        /// be executed here!
        /// </param>
        /// <returns>true, if the action was successful</returns>
        bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true);

        /// <summary>
        /// Executes follow-up steps after the execute method was run. Should be executed after a
        /// loop containing the Execute method.
        /// </summary>
        /// <param name="actionModifier">
        /// Optional modifier for the underlying class to do different things in the execute method
        /// </param>
        /// <param name="item">The item to be processed. The type is defined by the actionModifier.</param>
        /// <param name="isBulkAction">
        /// If true the action is executed for more than one game in a loop. Can be used to do
        /// things differently if only one game is processed.
        /// </param>
        /// <returns>true, if the action was successful</returns>
        void FollowUp(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true);

        /// <summary>
        /// Prepares the action before performing the execute method. Should be executed before a
        /// loop containing the Execute method.
        /// </summary>
        /// <param name="actionModifier">
        /// Optional modifier for the underlying class to do different things in the execute method
        /// </param>
        /// <param name="item">The item to be processed. The type is defined by the actionModifier.</param>
        /// <param name="isBulkAction">
        /// If true the action is executed for more than one game in a loop. Can be used to do
        /// things differently if only one game is processed.
        /// </param>
        /// <returns>true, if the action was successful</returns>
        bool Prepare(ActionModifierTypes actionModifier = ActionModifierTypes.None, object item = null, bool isBulkAction = true);
    }
}