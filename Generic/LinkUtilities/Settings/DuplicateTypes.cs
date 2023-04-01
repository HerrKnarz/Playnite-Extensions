using Playnite.SDK;
using System.Collections.Generic;

namespace LinkUtilities.Settings
{
    /// <summary>
    /// Types of values to check for duplicates.
    /// </summary>
    public enum DuplicateTypes
    {
        NameAndUrl,
        Name,
        Url
    }

    /// <summary>
    /// Dictionary of types with captions to show in a combo box.
    /// </summary>
    public class DuplicateTypesWithCaptions : Dictionary<DuplicateTypes, string>
    {
        public DuplicateTypesWithCaptions()
        {
            Add(DuplicateTypes.NameAndUrl, ResourceProvider.GetString("LOCLinkUtilitiesSettingsDuplicatesRemoveNameUrl"));
            Add(DuplicateTypes.Name, ResourceProvider.GetString("LOCLinkUtilitiesSettingsDuplicatesRemoveName"));
            Add(DuplicateTypes.Url, ResourceProvider.GetString("LOCLinkUtilitiesSettingsDuplicatesRemoveUrl"));
        }
    }
}