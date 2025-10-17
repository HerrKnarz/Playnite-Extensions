using KNARZhelper.Enum;
using KNARZhelper.MetadataCommon.DatabaseObjectTypes;
using KNARZhelper.MetadataCommon.Enum;
using Playnite.SDK.Data;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KNARZhelper.MetadataCommon
{
    /// <summary>
    /// Base class for metadata objects.
    /// </summary>
    public class BaseMetadataObject : DatabaseObject
    {
        private string _cleanedUpName;
        private string _editName;
        private int _gameCount;
        internal Guid _id;
        private string _name = string.Empty;
        private string _prefix = string.Empty;
        private bool _selected;
        private bool _showGrouped;
        internal FieldType _type;
        private IMetadataFieldType _typeManager;

        /// <summary>
        /// Creates a new instance of the BaseMetadataObject class.
        /// </summary>
        /// <param name="typeManager">Type manager of the metadata field</param>
        /// <param name="type">Type of the metadata field</param>
        /// <param name="name">Name of the metadata field</param>
        public BaseMetadataObject(IMetadataFieldType typeManager, FieldType type, string name = default)
        {
            TypeManager = typeManager;
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Creates a new instance of the BaseMetadataObject class.
        /// </summary>
        /// <param name="type">Type of the metadata field</param>
        /// <param name="name">Name of the metadata field</param>
        public BaseMetadataObject(FieldType type, string name = default)
        {
            Type = type;
            Name = name;
        }

        /// <summary>
        /// Gets or sets the cleaned up name of the metadata object.
        /// </summary>
        [DontSerialize]
        public string CleanedUpName
        {
            get => _cleanedUpName;
            set => SetValue(ref _cleanedUpName, value);
        }

        /// <summary>
        /// Gets the display name of the metadata object.
        /// </summary>
        [DontSerialize]
        public string DisplayName { get; private set; }

        /// <summary>
        /// Gets or sets the editable name of the metadata object.
        /// </summary>
        [DontSerialize]
        public string EditName
        {
            get => _editName;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_editName != value && UpdateItem(Prefix + value))
                {
                    SetValue(ref _editName, value);
                    _name = Prefix + value;
                    CleanedUpName = GetCleanedUpName();
                    DisplayName = GetDisplayName();
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the number of games associated with the metadata object.
        /// </summary>
        [DontSerialize]
        public int GameCount
        {
            get => _gameCount;
            set => SetValue(ref _gameCount, value);
        }

        /// <summary>
        ///  Gets or sets the ID of the metadata object.
        /// </summary>
        [DontSerialize]
        public new Guid Id
        {
            get
            {
                if (_id != default || !(TypeManager is IObjectType type))
                {
                    return _id;
                }

                _id = type.GetDbObjectId(Name);

                NotifyMissingItem();

                return _id;
            }
            set => SetValue(ref _id, value);
        }

        /// <summary>
        ///  Gets or sets the name of the metadata object.
        /// </summary>
        public new string Name
        {
            get => _name;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                SetValue(ref _name, value);

                _prefix = GetPrefix();

                if (value.Equals(_prefix))
                {
                    _prefix = string.Empty;
                    _editName = value;
                }
                else
                {
                    _editName = _prefix == string.Empty ? value : value.RemoveFirst(_prefix);
                }

                CleanedUpName = GetCleanedUpName();

                DisplayName = GetDisplayName();
            }
        }

        /// <summary>
        /// Specifies whether hidden games should be ignored when counting associated games.
        /// </summary>
        [DontSerialize]
        public virtual bool IgnoreHiddenGamesInCount => false;

        /// <summary>
        /// Gets or sets the prefix of the metadata object.
        /// </summary>
        [DontSerialize]
        public string Prefix
        {
            get => _prefix;
            set
            {
                if (value == null)
                {
                    value = string.Empty;
                }

                if (_prefix == value || !UpdateItem(value + EditName))
                {
                    return;
                }

                SetValue(ref _prefix, value);
                _name = value + EditName;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata object is selected.
        /// </summary>
        [DontSerialize]
        public bool Selected
        {
            get => _selected;
            set => SetValue(ref _selected, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the metadata object should be shown as grouped.
        /// </summary>
        [DontSerialize]
        public bool ShowGrouped
        {
            get => _showGrouped;
            set => SetValue(ref _showGrouped, value);
        }

        /// <summary>
        /// Gets or sets the type of the metadata object.
        /// </summary>
        public virtual FieldType Type
        {
            get => _type;
            set => SetValue(ref _type, value);
        }

        /// <summary>
        /// Gets the type label and name of the metadata object.
        /// </summary>
        [DontSerialize]
        public string TypeAndName => Name.Any() ? $"{TypeLabel}: {Name}" : TypeLabel;

        /// <summary>
        /// Label of the metadata object type.
        /// </summary>
        [DontSerialize]
        public string TypeLabel => TypeManager.LabelSingular;

        /// <summary>
        /// Gets or sets the type manager of the metadata object.
        /// </summary>
        [DontSerialize]
        public IMetadataFieldType TypeManager
        {
            get => _typeManager;
            set => SetValue(ref _typeManager, value);
        }

        /// <summary>
        /// Adds the metadata object to the database.
        /// </summary>
        /// <returns>Unique identifier of the added object</returns>
        public Guid AddToDb()
        {
            if (Name == null || Name == string.Empty)
            {
                return Guid.Empty;
            }

            if (TypeManager is IEditableObjectType editableType)
            {
                Id = editableType.AddDbObject(Name);
            }
            // If we can't add the item, for consistency we still get the id, since adding also returns the id.
            else
            {
                RefreshId();
            }

            return Id;
        }

        /// <summary>
        /// Adds the metadata object to the specified game.
        /// </summary>
        /// <param name="game">Game to add the object to</param>
        /// <returns>True if the object was added successfully</returns>
        public bool AddToGame(Game game) => TypeManager is IEditableObjectType type && type.AddValueToGame(game, Id);

        /// <summary>
        /// Checks if the metadata object is part of a group based on the provided groups dictionary and sets it to be shown in a group.
        /// </summary>
        /// <param name="groups">Dictionary to check for the object</param>
        public void CheckGroup(Dictionary<string, int> groups) => ShowGrouped = groups.TryGetValue(CleanedUpName, out var count) && count > 1;

        /// <summary>
        /// Checks if the metadata object exists in the database.
        /// </summary>
        /// <returns>True if the object exists</returns>
        public bool ExistsInDb() => TypeManager is IObjectType type && type.DbObjectExists(Name);

        /// <summary>
        /// Checks if the metadata object exists in the specified game.
        /// </summary>
        /// <param name="game">Game to check</param>
        /// <returns>True if the object was found in the game</returns>
        public bool ExistsInGame(Game game) => TypeManager is IValueType type && type.GameContainsValue(game, Id);

        /// <summary>
        /// Gets the cleaned up name of the metadata object.
        /// </summary>
        /// <returns>Cleaned up name</returns>
        public string GetCleanedUpName()
        {
            switch (Type)
            {
                case FieldType.Developer:
                case FieldType.Publisher:
                    if (string.IsNullOrEmpty(EditName))
                    {
                        return EditName;
                    }

                    var newName = EditName;

                    if (newName.EndsWith(")"))
                    {
                        newName = newName.Remove(newName.Length - 1);
                    }

                    newName = MiscHelper.CompanyFormRegex.Replace(newName, string.Empty).CollapseWhitespaces().Trim();

                    if (EditName.EndsWith(")"))
                    {
                        newName = $"{newName})";
                    }

                    return newName;
                default:
                    return EditName.RemoveDiacritics().RemoveSpecialChars().ToLower().Replace("-", "").Replace(" ", "");
            }
        }

        /// <summary>
        /// gets the display name of the metadata object.
        /// </summary>
        /// <returns></returns>
        public virtual string GetDisplayName() => Name;

        /// <summary>
        /// Gets the count of games associated with the metadata object and updates the GameCount property.
        /// </summary>
        public void GetGameCount()
        {
            if (TypeManager is IObjectType type)
            {
                GameCount = type.GetGameCount(Id, IgnoreHiddenGamesInCount);
            }
        }

        /// <summary>
        /// Gets the list of games associated with the metadata object.
        /// </summary>
        /// <returns>List of games</returns>
        public List<Game> GetGames()
        {
            return TypeManager is IObjectType type
                ? type.GetGames(Id, IgnoreHiddenGamesInCount)
                : new List<Game>();
        }

        /// <summary>
        /// Gets the prefix for the metadata object.
        /// </summary>
        /// <returns></returns>
        public virtual string GetPrefix() => string.Empty;

        /// <summary>
        /// Checks if the metadata object is part of the specified game.
        /// </summary>
        /// <param name="game">Game to check</param>
        /// <returns>True if the object was found in the game</returns>
        public bool IsInGame(Game game) => TypeManager is IObjectType type && type.DbObjectInGame(game, Id);

        /// <summary>
        /// Specifies whether the metadata object is used in any game.
        /// </summary>
        /// <param name="ignoreHiddenGames">Specifies if hodden games will be ignored when checking for the object.</param>
        /// <returns></returns>
        public bool IsUsed(bool ignoreHiddenGames = false) => TypeManager is IObjectType type && type.DbObjectInUse(Id, ignoreHiddenGames);

        /// <summary>
        /// Refreshes the ID of the metadata object from the database.
        /// </summary>
        public void RefreshId()
        {
            if (!(TypeManager is IObjectType type))
            {
                return;
            }

            Id = type.GetDbObjectId(Name);

            NotifyMissingItem();
        }

        /// <summary>
        /// Removes the metadata object from the specified game.
        /// </summary>
        /// <param name="game">Game the object will be removed from</param>
        /// <returns>True if the object was removed.</returns>
        public bool RemoveFromGame(Game game) => TypeManager is IEditableObjectType type && type.RemoveObjectFromGame(game, Id);

        /// <summary>
        /// Event that is triggered when the metadata object is renamed.
        /// </summary>
        public event RenameObjectEventHandler RenameObject;

        /// <summary>
        /// Updates the metadata object with a new name.
        /// </summary>
        /// <param name="newName">New name of the object</param>
        /// <returns>True when the object was renamed</returns>
        public bool UpdateItem(string newName) => Id == default || (RenameObject == null) || (RenameObject?.Invoke(this, Name, newName) ?? true);

        /// <summary>
        /// Updates the name of the metadata object in the database.
        /// </summary>
        /// <param name="newName">New name of the object</param>
        /// <returns>Database interaction result of the action</returns>
        public DbInteractionResult UpdateName(string newName)
        {
            return TypeManager is IEditableObjectType type
                ? type.UpdateName(Id, Name, newName)
                : DbInteractionResult.Error;
        }

        /// <summary>
        /// Notifies that the metadata object is missing in the database. Does nothing by default.
        /// </summary>
        public virtual void NotifyMissingItem()
        {
            // Implementation for notifying about a missing item
        }
    }
}
