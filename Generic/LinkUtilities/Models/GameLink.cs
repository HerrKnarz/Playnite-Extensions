using LinkUtilities.LinkActions;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;

namespace LinkUtilities.Models
{
    public class GameLink : ObservableObject
    {
        private Game _game;

        private Link _link;

        public Game Game
        {
            get => _game;
            set => SetValue(ref _game, value);
        }

        public Link Link
        {
            get => _link;
            set => SetValue(ref _link, value);
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