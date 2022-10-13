using Game = Playnite.SDK.Models.Game;

namespace LinkManager
{
    public enum LinkActionTypes
    {
        Sort,
        AddLibraryLink
    }

    public interface ILinkAction
    {
        LinkActionTypes Type { get; set; }
        string ProgressMessage { get; set; }
        string ResultMessage { get; set; }
        LinkManagerSettings Settings { get; set; }

        bool Execute(Game game);
    }

    public class SortLinks : ILinkAction
    {
        public LinkActionTypes Type { get; set; }
        public string ProgressMessage { get; set; }
        public string ResultMessage { get; set; }
        public LinkManagerSettings Settings { get; set; }

        public SortLinks(LinkManagerSettings settings)
        {
            Type = LinkActionTypes.Sort;
            ProgressMessage = "LOCLinkManagerLSortLinksProgress";
            ResultMessage = "LOCLinkManagerSortedMessage";
            Settings = settings;
        }

        public bool Execute(Game game)
        {
            return LinkHelper.SortLinks(game);
        }
    }

    public class AddLibraryLinks : ILinkAction
    {
        private readonly Libraries libraries;

        public LinkActionTypes Type { get; set; }
        public string ProgressMessage { get; set; }
        public string ResultMessage { get; set; }
        public LinkManagerSettings Settings { get; set; }

        public AddLibraryLinks(LinkManagerSettings settings)
        {
            Type = LinkActionTypes.Sort;
            ProgressMessage = "LOCLinkManagerLibraryLinkProgress";
            ResultMessage = "LOCLinkManagerAddedMessage";
            Settings = settings;

            libraries = new Libraries(Settings);
        }

        public bool Execute(Game game)
        {
            ILinkAssociation library;
            bool result = false;

            library = libraries.Find(x => x.AssociationId == game.PluginId);

            if (library is object)
            {
                if (library.AddLink(game))
                    result = true;
            }
            return result;
        }
    }
}
