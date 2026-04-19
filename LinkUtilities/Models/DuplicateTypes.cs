using Playnite;

namespace LinkUtilities.Models;

/// <summary>
/// Types of values to check for duplicates.
/// </summary>
public enum DuplicateTypes
{
    TypeAndUrl,
    Type,
    Url
}

/// <summary>
/// Dictionary of types with captions to show in a combo box.
/// </summary>
public class DuplicateTypesWithCaptions : Dictionary<DuplicateTypes, string>
{
    public DuplicateTypesWithCaptions()
    {
        Add(DuplicateTypes.TypeAndUrl, Loc.enum_duplicate_types_type_url());
        Add(DuplicateTypes.Type, Loc.enum_duplicate_types_type());
        Add(DuplicateTypes.Url, Loc.enum_duplicate_types_url());
    }
}