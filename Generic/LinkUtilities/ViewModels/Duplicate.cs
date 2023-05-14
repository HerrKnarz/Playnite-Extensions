using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;

namespace LinkUtilities
{
    public class Duplicate : ViewModelBase
    {
        private Game _game;

        private Link _link;

        private bool _mustUpdate;

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

        public bool MustUpdate
        {
            get => _mustUpdate;
            set
            {
                _mustUpdate = value;
                OnPropertyChanged("MustUpdate");
            }
        }

        public void Remove()
        {
            if (GlobalSettings.Instance().OnlyATest)
            {
                _mustUpdate |= _game.Links.Remove(_link);
            }
            else
            {
                API.Instance.MainView.UIDispatcher.Invoke(delegate
                {
                    _mustUpdate |= _game.Links.Remove(_link);
                });
            }
        }
    }
}