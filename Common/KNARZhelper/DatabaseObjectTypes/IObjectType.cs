using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    /// <summary>
    /// Defines functionality for object types in the database.
    /// </summary>
    /// <remarks>This interface extends <see cref="IMetadataFieldType"/> and <see cref="IGameInfoType"/>, providing additional methods for working with database objects.</remarks>
    public interface IObjectType : IMetadataFieldType, IGameInfoType
    {
        /// <summary>
        /// Gets the number of elements contained in the collection.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Defines whether the object type represents a list (multiple values) or a single value.
        /// </summary>
        bool IsList { get; }

        /// <summary>
        /// Checks if a database object with the specified name exists.
        /// </summary>
        /// <param name="name">Name of the database object.</param>
        /// <returns><see langword="true"/> if the database object exists; otherwise, <see langword="false"/>.</returns>
        bool DbObjectExists(string name);

        /// <summary>
        /// Checks if a database object with the specified ID exists.
        /// </summary>
        /// <param name="id">ID of the database object.</param>
        /// <returns><see langword="true"/> if the database object exists; otherwise, <see langword="false"/>.</returns>
        bool DbObjectExists(Guid id);

        /// <summary>
        /// Checks if a database object with the specified ID exists in the context of a game.
        /// </summary>
        /// <param name="game">Game instance to check against.</param>
        /// <param name="id">ID of the database object.</param>
        /// <returns><see langword="true"/> if the database object exists in the specified game; otherwise, <see langword="false"/>.</returns>
        bool DbObjectInGame(Game game, Guid id);

        /// <summary>
        /// Checks if a database object with the specified ID is in use by any game.
        /// </summary>
        /// <param name="id">ID of the database object.</param>
        /// <param name="ignoreHiddenGames">Whether to ignore hidden games.</param>
        /// <returns><see langword="true"/> if the database object is in use; otherwise, <see langword="false"/>.</returns>
        bool DbObjectInUse(Guid id, bool ignoreHiddenGames = false);

        /// <summary>
        /// Retrieves the unique identifier (GUID) of a database object based on its name.
        /// </summary>
        /// <param name="name">The name of the database object whose identifier is to be retrieved. Cannot be null or empty.</param>
        /// <returns>The unique identifier (GUID) of the database object if found; otherwise, an empty GUID.</returns>
        Guid GetDbObjectId(string name);

        /// <summary>
        /// Loads all database objects, excluding those specified in the provided set of identifiers.
        /// </summary>
        /// <remarks>This method retrieves all database objects except those whose identifiers are included in <paramref name="itemsToIgnore"/>.
        /// The caller is responsible for ensuring that  the provided set of identifiers is not null.</remarks>
        /// <param name="itemsToIgnore">A set of unique identifiers representing the database objects to exclude from the metadata loading process.</param>
        /// <returns>A list of <see cref="DatabaseObject"/> instances representing the loaded metadata. The list will be empty if
        /// no metadata is loaded.</returns>
        List<DatabaseObject> LoadAllMetadata(HashSet<Guid> itemsToIgnore);

        /// <summary>
        /// Loads metadata for the specified game, returning a list of database objects that represent the game's
        /// metadata.
        /// </summary>
        /// <param name="game">The game for which metadata is to be loaded. Cannot be <see langword="null"/>.</param>
        /// <param name="itemsToIgnore">An optional set of item identifiers to exclude from the results. If <see langword="null"/>, no items are
        /// excluded.</param>
        /// <returns>A list of <see cref="DatabaseObject"/> instances representing the metadata for the specified game. The list
        /// will be empty if no metadata is found.</returns>
        List<DatabaseObject> LoadGameMetadata(Game game, HashSet<Guid> itemsToIgnore = null);

        /// <summary>
        /// Loads all database objects that are not currently in use in a game.
        /// </summary>
        /// <param name="ignoreHiddenGames">A value indicating whether to exclude hidden games from the list of games to check.  <see langword="true"/> to ignore
        /// hidden games; otherwise, <see langword="false"/>.</param>
        /// <returns>A list of <see cref="DatabaseObject"/> instances representing unused metadata.  The list will be empty if no
        /// unused metadata is found.</returns>
        List<DatabaseObject> LoadUnusedMetadata(bool ignoreHiddenGames);

        /// <summary>
        /// Determines whether a name already exists in the system, ignoring a specific identifier.
        /// </summary>
        /// <remarks>This method is useful for checking name uniqueness when updating an existing object</remarks> 
        /// <param name="name">The name to check for existence. Cannot be null or empty.</param>
        /// <param name="idToIgnore">The unique identifier to exclude from the check.</param>
        /// <returns><see langword="true"/> if the name exists and is associated with an identifier other than <paramref
        /// name="idToIgnore"/>; otherwise, <see langword="false"/>.</returns>
        bool NameExists(string name, Guid idToIgnore);
    }
}