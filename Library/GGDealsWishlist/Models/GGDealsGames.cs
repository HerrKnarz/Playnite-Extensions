using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GGDealsWishlist.Models
{
    /// <summary>
    /// Represents a collection of GGDealsGame objects, providing methods to retrieve game metadata
    /// and filter new games based on their import status.
    /// </summary>
    public class GGDealsGames : ObservableCollection<GGDealsGame>
    {
        private readonly Settings _settings;

        public GGDealsGames(Settings settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Timestamp of the last refresh of the GGDealsGames collection.
        /// </summary>
        public DateTime LastRefresh { get; set; } = DateTime.MinValue;

        /// <summary>
        /// Retrieves a list of GameMetadata objects from the GGDealsGames collection. Used to
        /// import the games into Playnite.
        /// </summary>
        /// <returns>List of GameMetadata objects.</returns>
        public List<GameMetadata> GetGameMetadataList() => this.Select(g => g.ImportedMetadata).ToList();

        /// <summary>
        /// Retrieves a list of GameMetadata objects for games that have not yet been imported into
        /// Playnite. The method filters the collection to include only those games where the Game
        /// property is null, indicating they are new and not yet imported. The maxGames parameter
        /// allows limiting the number of new games returned.
        /// </summary>
        /// <param name="maxGames">Maximum number of new games to return.</param>
        /// <returns>List of new GameMetadata objects.</returns>
        public List<GameMetadata> GetNewGames(int maxGames = 0)
        {
            var list = this
                .Where(ng => ng.Game is null)
                .Take(maxGames)
                .Select(g => g.ImportedMetadata)
                .ToList();

            return maxGames > 0 ? list.Take(maxGames).ToList() : list;
        }
    }
}
