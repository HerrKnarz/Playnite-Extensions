using LinkUtilities;
using Playnite.Markup;

namespace Playnite;

public class LocString : LocStringMarkup
{
    public LocString() : base(LinkUtilitiesPlugin.Id)
    {
    }

    public LocString(string stringId) : base(LinkUtilitiesPlugin.Id, stringId)
    {
    }
}

//NEXT: Replace generic strings with the ones from Playnite's localization file.

public static partial class Loc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static IPlayniteApi Api = null!;

    public static string GetString(string stringId) => Api.GetLocalizedString(stringId);

    public static string GetString(string stringId, params (string name, object value)[] args) => Api.GetLocalizedString(stringId, args);

    public static bool IsStringId(string id) => Api.IsLocalizedStringId(id);
}