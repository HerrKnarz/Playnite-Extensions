using LinkUtilities;
using Playnite.Markup;

namespace Playnite;

public class LocalizedString : LocStringMarkup
{
    public LocalizedString() : base(LinkUtilitiesPlugin.Id)
    {
    }

    public LocalizedString(string stringId) : base(LinkUtilitiesPlugin.Id, stringId)
    {
    }
}

public static partial class Loc
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible", Justification = "<Pending>")]
    public static IPlayniteApi Api = null!;

    public static string GetString(string stringId) => Api.GetLocalizedString(stringId);

    public static string GetString(string stringId, params (string name, object value)[] args) => Api.GetLocalizedString(stringId, args);

    public static bool IsStringId(string id) => LocId.StringIds.Contains(id);
}