using Playnite.SDK;
using System.Diagnostics;

namespace GGDealsWishlist
{
    public class GGDealsWishlistClient : LibraryClient
    {
        public override string Icon => GGDealsWishlist.Icon;

        public override bool IsInstalled => true;

        public override void Open() => Process.Start(new ProcessStartInfo("https://gg.deals"));
    }
}
