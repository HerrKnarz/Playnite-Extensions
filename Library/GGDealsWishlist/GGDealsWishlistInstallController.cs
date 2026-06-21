using KNARZhelper.MetadataCommon;
using Playnite.SDK.Models;
using Playnite.SDK.Plugins;
using System.Diagnostics;

namespace GGDealsWishlist
{
    internal class GGDealsWishlistInstallController : InstallController
    {
        public GGDealsWishlistInstallController(Game game) : base(game)
        {
            Name = "Open gg.deals link";
        }

        public override void Install(InstallActionArgs args)
        {
            InvokeOnInstallationCancelled(new GameInstallationCancelledEventArgs());

            var link = MetadataHelper.GetLink(Game, new System.Text.RegularExpressions.Regex(@"gg\.deals\/game\/"));

            if (link != null)
            {
                Process.Start(new ProcessStartInfo(link.Url));
            }
        }
    }
}
