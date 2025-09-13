using KNARZhelper.Enum;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;

namespace KNARZhelper.DatabaseObjectTypes
{
    /// <summary>
    /// Represents the method that will handle an event triggered when an object is renamed.
    /// </summary>
    /// <param name="sender">The source of the event, typically the object being renamed.</param>
    /// <param name="oldName">The original name of the object before the rename operation.</param>
    /// <param name="newName">The new name assigned to the object.</param>
    /// <returns><see langword="true"/> if the rename operation is allowed to proceed; otherwise, <see langword="false"/>.</returns>
    public delegate bool RenameObjectEventHandler(object sender, string oldName, string newName);

    /// <summary>
    /// Defines functionality for editable object types in the database.
    /// </summary>
    public interface IEditableObjectType : IObjectType, IValueType
    {
        /// <summary>
        /// Event that is triggered when an object is renamed.
        /// </summary>
        event RenameObjectEventHandler RenameObject;

        /// <summary>
        /// Adds a new database object with the specified name and returns its unique identifier.
        /// </summary>
        /// <param name="name">The name of the database object to add. Cannot be null or empty.</param>
        /// <returns>A <see cref="Guid"/> representing the unique identifier of the newly added database object.</returns>
        Guid AddDbObject(string name);

        /// <summary>
        /// Removes the database object with the specified identifier.
        /// </summary>
        /// <remarks>This method does not throw an exception if the object with the specified identifier
        /// does not exist;  instead, it returns <see langword="false"/>.</remarks>
        /// <param name="id">The unique identifier of the database object to remove.</param>
        /// <returns><see langword="true"/> if the object was successfully removed; otherwise, <see langword="false"/>.</returns>
        bool RemoveDbObject(Guid id);

        /// <summary>
        /// Removes objects with the specified identifiers from the given game.
        /// </summary>
        /// <remarks>If any identifier in <paramref name="ids"/> does not correspond to an object in the
        /// game, the method will return <see langword="false"/> and no objects will be removed.</remarks>
        /// <param name="game">The game instance from which the objects will be removed.</param>
        /// <param name="ids">A list of unique identifiers representing the objects to remove.</param>
        /// <returns><see langword="true"/> if all specified objects were successfully removed; otherwise, <see
        /// langword="false"/>.</returns>
        bool RemoveObjectFromGame(Game game, List<Guid> ids);

        /// <summary>
        /// Removes an object from the specified game using its unique identifier.
        /// </summary>
        /// <remarks>If the object with the specified <paramref name="id"/> does not exist in the game,
        /// the method returns <see langword="false"/>.</remarks>
        /// <param name="game">The game instance from which the object will be removed. Cannot be <see langword="null"/>.</param>
        /// <param name="id">The unique identifier of the object to remove.</param>
        /// <returns><see langword="true"/> if the object was successfully removed; otherwise, <see langword="false"/>.</returns>
        bool RemoveObjectFromGame(Game game, Guid id);

        /// <summary>
        /// Removes the specified object, identified by its unique identifier, from all games in the provided list.
        /// </summary>
        /// <remarks>This method iterates through the provided list of games and removes the object with
        /// the specified identifier from each game. The returned collection can be used to determine which games were
        /// affected by the operation.</remarks>
        /// <param name="games">The list of games from which the object will be removed. Cannot be null.</param>
        /// <param name="id">The unique identifier of the object to remove.</param>
        /// <returns>An enumerable collection of game identifiers representing the games from which the object was successfully
        /// removed. If the object was not found in any game, the collection will be empty.</returns>
        IEnumerable<Guid> RemoveObjectFromGames(List<Game> games, Guid id);

        /// <summary>
        /// Replaces the specified database object in the provided list of games with a new object type and/or identifier.
        /// </summary>
        /// <param name="games">The list of games in which the object will be replaced. Cannot be null.</param>
        /// <param name="id">The unique identifier of the object to replace.</param>
        /// <param name="newType">The new object type to assign to the database object. If null, the type will not be changed.</param>
        /// <param name="newId">The new unique identifier to assign to the database object. If null, the ID will not be changed.</param>
        /// <returns>An enumerable collection of game identifiers representing the games in which the object was successfully replaced.
        /// If the object was not found in any game, the collection will be empty.</returns>
        IEnumerable<Guid> ReplaceDbObject(List<Game> games, Guid id, IEditableObjectType newType = null, Guid? newId = null);

        /// <summary>
        /// Updates the database object with the specified identifier and name.
        /// </summary>
        /// <param name="id">The unique identifier of the database object to update.</param>
        /// <param name="name">The new name to assign to the database object. Cannot be null or empty.</param>
        void UpdateDbObject(Guid id, string name);

        /// <summary>
        /// Updates the name of an entity identified by the specified ID.
        /// </summary>
        /// <param name="id">The unique identifier of the entity whose name is to be updated.</param>
        /// <param name="oldName">The current name of the entity. This value is used to verify the entity's existing state.</param>
        /// <param name="newName">The new name to assign to the entity. Cannot be null or empty.</param>
        /// <returns>A <see cref="DbInteractionResult"/> indicating the outcome of the operation, including success or failure
        /// details.</returns>
        DbInteractionResult UpdateName(Guid id, string oldName, string newName);
    }
}