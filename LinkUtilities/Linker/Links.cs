using LinkUtilities.Linker.Libraries;
using LinkUtilities.Linker.LinkSources;

namespace LinkUtilities.Linker;

public class LinkDict : Dictionary<string, Func<BaseLinkSource>>
{
    public LinkDict()
    {
        Add(LinkAdventureGamers.ClassId, () => new LinkAdventureGamers(LinkAdventureGamers.ClassId, new LinkSourceArgs()));
        Add(LinkBackloggd.ClassId, () => new LinkBackloggd(LinkBackloggd.ClassId, new LinkSourceArgs()));
        Add(LinkDoomWiki.ClassId, () => new LinkDoomWiki(LinkDoomWiki.ClassId, new LinkSourceArgs()));
        Add(LinkEpic.ClassId, () => new LinkEpic(LinkEpic.ClassId, new LinkSourceArgs()));
        Add(LinkGameFaqs.ClassId, () => new LinkGameFaqs(LinkGameFaqs.ClassId, new LinkSourceArgs()));
        Add(LinkGgDeals.ClassId, () => new LinkGgDeals(LinkGgDeals.ClassId, new LinkSourceArgs()));
        Add(LibraryLinkGog.ClassId, () => new LibraryLinkGog(LibraryLinkGog.ClassId, new LinkSourceArgs()));
        Add(LinkIgn.ClassId, () => new LinkIgn(LinkIgn.ClassId, new LinkSourceArgs()));
        Add(LinkMobyGames.ClassId, () => new LinkMobyGames(LinkMobyGames.ClassId, new LinkSourceArgs()));
        Add(LinkProtonDb.ClassId, () => new LinkProtonDb(LinkProtonDb.ClassId, new LinkSourceArgs()));
        Add(LinkSteamDb.ClassId, () => new LinkSteamDb(LinkSteamDb.ClassId, new LinkSourceArgs()));
        Add(LinkSteamPeek.ClassId, () => new LinkSteamPeek(LinkSteamPeek.ClassId, new LinkSourceArgs()));
    }
}

public class LinkSourceArgs
{
}

public class Links : List<BaseLinkSource>
{
    public Links(List<BaseLinkSource>? links = null)
    {
        if (links is null)
        {
            var linkDict = new LinkDict();

            AddRange(linkDict.Select(l => l.Value()));
        }
        else
        {
            AddRange(links);
        }

        /* Add(new LinkArcadeDatabase());       */
        /* Add(new LinkCoOptimus());            */
        /* Add(new LinkFamilyGamingDatabase()); */
        /* Add(new LinkGamePressureGuides());     */
        /* Add(new LinkGamerGuides());            */
        /* Add(new LinkGiantBomb());              */
        /* Add(new LinkGogDb());                  */
        /* Add(new LinkGrouvee());                */
        /* Add(new LinkHg101());                  */
        /* Add(new LinkIGDB());                   */
        /* Add(new LinkIgnGuides());              */
        /* Add(new LinkIsThereAnyDeal());         */
        /* Add(new LinkKillerListOfVideoGames()); */
        /* Add(new LibraryLinkItch());            */
        /* Add(new LinkLemon64());                */
        /* Add(new LinkLemonAmiga());             */
        /* Add(new LinkMapGenie());               */
        /* Add(new LinkMetacritic());             */
        /* Add(new LinkModDb());                  */
        /* Add(new LinkNecRetro());               */
        /* Add(new LinkNintendoWiki());           */
        /* Add(new LinkPcGamingWiki());           */
        /* Add(new LinkRawg());                   */
        /* Add(new LinkSegaRetro());              */
        /* Add(new LinkSnkWiki());                */
        /* Add(new LibraryLinkSteam());           */
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