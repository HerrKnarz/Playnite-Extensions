using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using System.Linq;

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

        public void Replace()
        {
            if (GlobalSettings.Instance().OnlyATest)
            {
                Game.Links.Single(x => x.Name == Link.Name).Url = LinkCheckResult.ResponseUrl;
            }
            else
            {
                API.Instance.MainView.UIDispatcher.Invoke(delegate
                {
                    Game.Links.Single(x => x.Name == Link.Name).Url = LinkCheckResult.ResponseUrl;

                    API.Instance.Database.Games.Update(Game);
                });
            }

            UrlIsEqual = true;
        }
    }
}