using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GGDealsWishlist.Models
{
    public class GGDealsGames : ObservableCollection<GGDealsGame>
    {
        public DateTime LastRefresh { get; set; } = DateTime.MinValue;

        public List<GameMetadata> GetGameMetadataList() => this.Select(g => (GameMetadata)g).ToList();

        public List<GameMetadata> GetNewGames(int maxGames = 0)
        {
            var list = this
                .Where(ng => ng.Game is null)
                .Take(maxGames)
                .Select(g => (GameMetadata)g)
                .ToList();

            return maxGames > 0 ? list.Take(maxGames).ToList() : list;
        }
    }
}
