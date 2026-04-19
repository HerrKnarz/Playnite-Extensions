using Playnite;

namespace LinkUtilities.Models;

/// <summary>
/// Model for the search results of a link search
/// </summary>
public class LinkSearchResult(string name = "", string? description = null) : ChooseDialogItem(name, description)
{
    /// <summary>
    /// ID of the game on the external website. Can be used to add an external ID to the game.
    /// </summary>
    public string? Id;

    /// <summary>
    /// URL of the link
    /// </summary>
    public string? Url;
}