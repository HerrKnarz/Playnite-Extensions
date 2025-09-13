using KNARZhelper.Enum;
using Playnite.SDK;
using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace KNARZhelper.DatabaseObjectTypes
{
    public class TypeLink : IMetadataFieldType, IValueType, IClearAbleType
    {
        public event RenameObjectEventHandler RenameObject
        {
            add { }
            remove { }
        }

        public bool CanBeAdded => false;

        public bool CanBeClearedInGame => true;

        public bool CanBeDeleted => false;

        public bool CanBeEmptyInGame => true;

        public bool CanBeModified => false;

        public bool CanBeSetByMetadataAddOn => true;

        public bool CanBeSetInGame => true;

        public bool IsDefaultToCopy => true;

        public string LabelPlural => ResourceProvider.GetString("LOCLinksLabel");

        public string LabelSingular => ResourceProvider.GetString("LOCGameActionTypeLink");

        public FieldType Type => FieldType.Link;

        public ItemValueType ValueType => ItemValueType.LinkList;

        public bool AddValueToGame<T>(Game game, T value) => false; // TODO: implement

        public void EmptyFieldInGame(Game game) => game.Links?.Clear();

        public bool FieldInGameIsEmpty(Game game) => game.Links == null || game.Links?.Count == 0;

        public bool GameContainsValue<T>(Game game, T value) => false; // TODO: implement

        public bool CopyValueToGame(Game sourceGame, Game targetGame, bool replaceValue = false, bool onlyIfEmpty = false)
        {
            if (sourceGame?.Links == null || sourceGame.Links.Count == 0)
            {
                return false;
            }

            if (onlyIfEmpty && !FieldInGameIsEmpty(targetGame))
            {
                return false;
            }

            if (replaceValue)
            {
                EmptyFieldInGame(targetGame);
            }

            if (targetGame.Links == null)
            {
                targetGame.Links = new ObservableCollection<Link>();
            }

            targetGame.Links.AddMissing(sourceGame.Links.Select(link => new Link(link.Name, link.Url)));

            return true;
        }
    }
}