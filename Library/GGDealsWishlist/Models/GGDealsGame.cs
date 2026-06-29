using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.IO;

namespace GGDealsWishlist.Models
{
    public class GGDealsGame : ObservableObject
    {
        private DiscountData _discountData;
        private Game _game;
        private string _gGDealsCoverLink;
        private GameMetadata _importedMetadata;

        public DiscountData DiscountData
        {
            get => _discountData;
            set => SetValue(ref _discountData, value);
        }

        public string DisplayImage
        {
            get
            {
                if (Game?.CoverImage is null)
                {
                    return GGDealsCoverLink;
                }

                var fileInfo = new FileInfo(API.Instance.Database.GetFullFilePath(Game.CoverImage));

                return fileInfo.Exists ? fileInfo.FullName : GGDealsCoverLink;
            }
        }

        public string DisplayName => Game?.Name ?? ImportedMetadata.Name;

        public Game Game
        {
            get => _game;
            set => SetValue(ref _game, value);
        }

        public string GGDealsCoverLink
        {
            get => _gGDealsCoverLink;
            set => SetValue(ref _gGDealsCoverLink, value);
        }

        public GameMetadata ImportedMetadata
        {
            get => _importedMetadata;
            set => SetValue(ref _importedMetadata, value);
        }
    }
}
