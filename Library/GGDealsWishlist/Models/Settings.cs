using System.Collections.Generic;

namespace GGDealsWishlist.Models
{
    public class Settings : ObservableObject
    {
        private string _defaultCategory = string.Empty;
        private bool _importGamesAsInstalled = false;
        private int _maxGamesToImport = 100;
        private string _wishlistUrl = string.Empty;

        public string DefaultCategory
        {
            get => _defaultCategory;
            set => SetValue(ref _defaultCategory, value);
        }

        public bool ImportGamesAsInstalled
        {
            get => _importGamesAsInstalled;
            set => SetValue(ref _importGamesAsInstalled, value);
        }

        public int MaxGamesToImport
        {
            get => _maxGamesToImport;
            set
            {
                if (value < 0)
                {
                    value = 0;
                }

                SetValue(ref _maxGamesToImport, value);
            }
        }

        public string WishlistUrl
        {
            get => _wishlistUrl;
            set => SetValue(ref _wishlistUrl, value);
        }
    }
}
