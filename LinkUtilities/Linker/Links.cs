using LinkUtilities.Linker.Libraries;
using LinkUtilities.Linker.LinkSources;

namespace LinkUtilities.Linker;

public class Links : List<BaseClasses.Linker>
{
    public Links()
    {
        /* Add(new LinkAdventureGamers());      */
        /* Add(new LinkArcadeDatabase());       */
        /* Add(new LinkBackloggd());            */
        /* Add(new LinkCoOptimus());            */
        /* Add(new LinkDoomWiki());             */
        /* Add(new LinkEpic());                 */
        /* Add(new LinkFamilyGamingDatabase()); */
        Add(new LinkGameFaqs());
        /* Add(new LinkGamePressureGuides());     */
        /* Add(new LinkGamerGuides());            */
        Add(new LinkGgDeals());
        /* Add(new LinkGiantBomb());              */
        Add(new LibraryLinkGog());
        /* Add(new LinkGogDb());                  */
        /* Add(new LinkGrouvee());                */
        /* Add(new LinkHg101());                  */
        /* Add(new LinkIGDB());                   */
        Add(new LinkIgn());
        /* Add(new LinkIgnGuides());              */
        /* Add(new LinkIsThereAnyDeal());         */
        /* Add(new LinkKillerListOfVideoGames()); */
        /* Add(new LibraryLinkItch());            */
        /* Add(new LinkLemon64());                */
        /* Add(new LinkLemonAmiga());             */
        /* Add(new LinkMapGenie());               */
        /* Add(new LinkMetacritic());             */
        Add(new LinkMobyGames());
        /* Add(new LinkModDb());                  */
        /* Add(new LinkNecRetro());               */
        /* Add(new LinkNintendoWiki());           */
        /* Add(new LinkPcGamingWiki());           */
        Add(new LinkProtonDb());
        /* Add(new LinkRawg());                   */
        /* Add(new LinkSegaRetro());              */
        /* Add(new LinkSnkWiki());                */
        /* Add(new LibraryLinkSteam());           */
        Add(new LinkSteamDb());
        Add(new LinkSteamPeek());
        /* Add(new LinkStrategyWiki());  */
        /* Add(new LinkTvTropes());      */
        /* Add(new LinkVndb());          */
        /* Add(new LinkWikipedia());     */
        /* Add(new LinkZopharsDomain()); */
    }

    public async Task InitializeAsync()
    {
        foreach (var link in this.Where(link => !link.Initialized))
        {
            await link.InitializeAsync();
        }
    }

    /*public void RefreshCustomLinkProfiles(List<CustomLinkProfile> customLinkProfiles)
    {
        RemoveAll(x => x.Settings.IsCustomSource);

        AddRange(customLinkProfiles.Select(x => new CustomLinker(x)));
    }*/
    //NEXT: Implement RefreshCustomLinkProfiles
}