using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GGDealsWishlist.Models
{
    public class GGDealsGames : ObservableCollection<GGDealsGame>
    {
        public List<GameMetadata> GetGameMetadataList() => this.Select(g => (GameMetadata)g).ToList();

        public List<GameMetadata> GetNewGames() => this.Where(ng => !API.Instance.Database.Games.Any(g => g.GameId == ng.GameId && g.PluginId == GGDealsWishlist.PluginId)).Select(g => (GameMetadata)g).ToList();
    }
}
