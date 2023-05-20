using LinkUtilities.LinkActions;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace LinkUtilities
{
    public class LinkViewModel : ViewModelBase
    {
        private Game _game;

        private Link _link;

        public Game Game
        {
            get => _game;
            set
            {
                _game = value;
                OnPropertyChanged("Game");
            }
        }

        public Link Link
        {
            get => _link;
            set
            {
                _link = value;
                OnPropertyChanged("Link");
            }
        }

        public void Remove()
        {
            if (GlobalSettings.Instance().OnlyATest)
            {
                _game.Links.Remove(_link);

                if (TagMissingLinks.Instance().TagMissingLinksAfterChange)
                {
                    TagMissingLinks.Instance().Execute(_game);
                }
            }
            else
            {
                API.Instance.MainView.UIDispatcher.Invoke(delegate
                {
                    if (!_game.Links.Remove(_link))
                    {
                        return;
                    }

                    if (TagMissingLinks.Instance().TagMissingLinksAfterChange)
                    {
                        TagMissingLinks.Instance().Execute(_game);
                    }

                    API.Instance.Database.Games.Update(_game);
                });
            }
        }
    }
}