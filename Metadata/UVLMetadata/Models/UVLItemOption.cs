using Playnite.SDK;

namespace UVLMetadata.Models;

/// <summary>
/// Search results for UVL searches with added key property.
/// </summary>
public class UVLItemOption : GenericItemOption
{
    public string Platform;

    /// <summary>
    /// unique URL of the page
    /// </summary>
    public string Url;
}
