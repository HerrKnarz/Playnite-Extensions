using LinkUtilities.LinkActions;
using LinkUtilities.Linker.Libraries;

namespace LinkUtilities.Linker;

/// <summary>
/// List of all game library link associations. Is used to get the specific library of the game via
/// the GUID.
/// </summary>
internal class LibraryLinks : Dictionary<string, LibraryLinker>
{
    public LibraryLinks()
    {
        //var steamLib = new LibraryLinkSteam();
        //Add(steamLib.Id, steamLib);

        Add(LibraryLinkGog.ClassId, new LibraryLinkGog(LibraryLinkGog.ClassId, new LinkSourceArgs()));

        //var itchLib = new LibraryLinkItch();
        //Add(itchLib.Id, itchLib);
    }
}