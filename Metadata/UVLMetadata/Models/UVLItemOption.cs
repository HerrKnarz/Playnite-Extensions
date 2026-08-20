using Playnite.SDK;

namespace UVLMetadata.Models;

/// <summary>
/// Search results for UVL searches with added properties needed to process them.
/// </summary>
public class UVLItemOption : GenericItemOption
{
    public string DeflatedName { get; set; }
    public string Platform { get; set; }
    public string ReleaseDate { get; set; }
    public string Url { get; set; }
}
