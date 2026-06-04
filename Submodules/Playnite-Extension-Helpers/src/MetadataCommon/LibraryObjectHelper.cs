using Playnite;

namespace PlayniteExtensionHelpers.MetadataCommon;

public static class LibraryObjectHelper
{
    public static async Task<LibraryObject?> GetLibraryObjectAsync<T>(ILibraryCollection<T> libraryCollection, string? name = null, string? typeId = null, bool createNew = true) where T : LibraryObject, new()
    {
        if (name.IsNullOrEmpty() && typeId.IsNullOrEmpty())
        {
            return null;
        }

        T? type = null;

        if (!typeId.IsNullOrEmpty())
        {
            type = libraryCollection.Get(typeId);
        }

        if (type == null && !name.IsNullOrEmpty())
        {
            type = libraryCollection.FirstOrDefault(t => t.Name == name);
        }

        if (type == null && !name.IsNullOrEmpty())
        {
            if (typeId.IsNullOrEmpty())
            {
                typeId = name.ToTypeId() ?? string.Empty;
            }

            type = libraryCollection.Get(typeId);
        }

        if (type == null && !typeId.IsNullOrEmpty() && !name.IsNullOrEmpty() && createNew)
        {
            var newType = new T
            {
                Name = name,
                Id = typeId
            };

            await libraryCollection.AddAsync(newType);

            return newType;
        }

        return type;
    }

    /// <summary>
    /// String extension to generate a type id from a name. Removes special characters, replaces
    /// whitespaces with dots and converts to lower case.
    /// </summary>
    /// <param name="name">The name to convert to a type id</param>
    /// <returns>The generated type id</returns>
    public static string? ToTypeId(this string? name) => name.RemoveSpecialChars()?.ToLower()?.Replace("-", " ").CollapseWhitespaces()?.Replace(" ", ".");
}