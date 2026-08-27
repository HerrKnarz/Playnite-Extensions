using Playnite.SDK;
using System.Collections.Generic;

namespace UVLMetadata.Enums;

public enum AuthenticationStatus
{
    NotAuthenticated,
    Authenticated,
    Unknown
}

public class AuthenticationStatusModes : Dictionary<AuthenticationStatus, string>
{
    public AuthenticationStatusModes()
    {
        Add(AuthenticationStatus.NotAuthenticated, ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusNotAuthenticated"));
        Add(AuthenticationStatus.Authenticated, ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusAuthenticated"));
        Add(AuthenticationStatus.Unknown, ResourceProvider.GetString("LOCUVLMetadataAuthenticationStatusCheckingStatus"));
    }
}

public class AuthenticationStatusButtonModes : Dictionary<AuthenticationStatus, string>
{
    public AuthenticationStatusButtonModes()
    {
        Add(AuthenticationStatus.NotAuthenticated, ResourceProvider.GetString("LOCUVLMetadataSettingsButtonAuthenticate"));
        Add(AuthenticationStatus.Authenticated, ResourceProvider.GetString("LOCUVLMetadataSettingsButtonLogout"));
        Add(AuthenticationStatus.Unknown, ResourceProvider.GetString("LOCUVLMetadataSettingsButtonAuthenticate"));
    }
}
