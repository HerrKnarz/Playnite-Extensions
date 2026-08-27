using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum DescriptionToUse
{
    Description,
    OfficialDescription,
    Both,
}

public class DescriptionToUseModes : Dictionary<DescriptionToUse, string>
{
    public DescriptionToUseModes()
    {
        Add(DescriptionToUse.OfficialDescription, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseOfficialDescription"));
        Add(DescriptionToUse.Description, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseDescription"));
        Add(DescriptionToUse.Both, ResourceProvider.GetString("LOCUVLMetadataSettingsDescriptionToUseBoth"));
    }
}
