using LinkUtilities.BaseClasses;
using LinkUtilities.Interfaces;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Sorts the Links of a game.
    /// </summary>
    internal class SortLinks : LinkAction
    {
        private static SortLinks _instance;
        private SortLinks() { }

        public override string ProgressMessage => "LOCLinkUtilitiesProgressSortLinks";
        public override string ResultMessage => "LOCLinkUtilitiesDialogSortedMessage";

        public bool SortAfterChange { get; set; } = false;

        public Dictionary<string, int> SortOrder { get; set; }

        public bool UseCustomSortOrder { get; set; } = false;

        public static SortLinks Instance() => _instance ?? (_instance = new SortLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               (actionModifier == ActionModifierTypes.SortOrder ||
                (actionModifier == ActionModifierTypes.None && UseCustomSortOrder)
                   ? Sort(game, SortOrder)
                   : Sort(game));

        /// <summary>
        ///     Gets the sort position of a link name in the dictionary. If nothing is found, max int is returned, so the link will
        ///     be last.
        /// </summary>
        /// <param name="linkName">Name of the link to be sorted</param>
        /// <param name="sortOrder">Dictionary that contains the sort order.</param>
        /// <returns>
        ///     Position in the sort order. The max int is returned, if the link name is not in the dictionary. That way
        ///     those links will always appear after the defined order.
        /// </returns>
        private static int? GetSortPosition(string linkName, IReadOnlyDictionary<string, int> sortOrder) =>
            sortOrder.TryGetValue(linkName, out var position) ? position : int.MaxValue;

        /// <summary>
        ///     Sorts the Links of a game alphabetically by the link name.
        /// </summary>
        /// <param name="game">Game in which the links will be sorted.</param>
        /// <returns>True, if the links could be sorted</returns>
        private static bool Sort(Game game)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => x.Name));

            if (!GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }

        /// <summary>
        ///     Sorts the Links of a game according to a defined sort order.
        /// </summary>
        /// <param name="game">Game in which the links will be sorted.</param>
        /// <param name="sortOrder">Dictionary that contains the sort order.</param>
        /// <returns>True, if the links could be sorted</returns>
        private static bool Sort(Game game, IReadOnlyDictionary<string, int> sortOrder)
        {
            if (!game.Links?.Any() ?? true)
            {
                return false;
            }

            game.Links = new ObservableCollection<Link>(game.Links.OrderBy(x => GetSortPosition(x.Name, sortOrder))
                .ThenBy(x => x.Name));

            if (!GlobalSettings.Instance().OnlyATest)
            {
                API.Instance.Database.Games.Update(game);
            }

            return true;
        }
    }
}