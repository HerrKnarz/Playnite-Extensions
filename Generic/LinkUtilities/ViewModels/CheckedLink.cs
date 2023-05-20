using LinkUtilities.Models;

namespace LinkUtilities
{
    internal class CheckedLink : LinkViewModel
    {
        private LinkCheckResult _linkCheckResult;
        private bool _urlIsEqual;

        public LinkCheckResult LinkCheckResult
        {
            get => _linkCheckResult;
            set
            {
                _linkCheckResult = value;
                OnPropertyChanged("LinkCheckResult");
            }
        }

        public bool UrlIsEqual
        {
            get => _urlIsEqual;
            set
            {
                _urlIsEqual = value;
                OnPropertyChanged("UrlIsEqual");
            }
        }
    }
}