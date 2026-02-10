using KNARZhelper.WebCommon.Models;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using System.Linq;

namespace LinkUtilities.ViewModels
{
    public class CheckGameLink : GameLink
    {
        private UrlLoadResult _linkCheckResult;
        private bool _urlIsEqual;

        public UrlLoadResult LinkCheckResult
        {
            get => _linkCheckResult;
            set => SetValue(ref _linkCheckResult, value);
        }

        public bool UrlIsEqual
        {
            get => _urlIsEqual;
            set => SetValue(ref _urlIsEqual, value);
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