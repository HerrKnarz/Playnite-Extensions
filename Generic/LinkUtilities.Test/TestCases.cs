using LinkUtilities.BaseClasses;
using LinkUtilities.Linker;
using System.Collections;
using System.Collections.Generic;

namespace LinkUtilities.Test
{
    internal class TestCase
    {
        public string CaseName { get; set; }
        public Link Link { get; set; }
        public string GameName { get; set; }
        public string GamePath { get; set; }
        public string Url { get; set; }
        public string SearchedUrl { get; set; }
    }

    internal class TestCases : IEnumerable<object[]>
    {
        private readonly List<object[]> _data = new List<object[]>
        {
            new object[]
            {
                new TestCase()
                {
                    CaseName = "AdventureGamers",
                    Link = new LinkAdventureGamers(),
                    GameName = "Day of the Tentacle",
                    GamePath = "https://adventuregamers.com/games/view/15427",
                    Url = "https://adventuregamers.com/games/view/15427",
                    SearchedUrl = "https://adventuregamers.com/games/view/15427"
                }
            },
            // Todo: add ArcadeDatabase -> needs roms though!
            new object[]
            {
                new TestCase()
                {
                    CaseName = "CoOptimus",
                    Link = new LinkCoOptimus(),
                    GameName = "Minecraft",
                    GamePath = "https://www.co-optimus.com/game/3006/Android/minecraft.html",
                    Url = "https://www.co-optimus.com/game/3006/Android/minecraft.html",
                    SearchedUrl = "https://www.co-optimus.com/game/2544/Xbox 360/minecraft-xbox-360-edition.html"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "gamepressure Guides",
                    Link = new LinkGamePressureGuides(),
                    GameName = "Assassin's Creed: Odyssey",
                    GamePath = "assassins-creed-odyssey",
                    Url = "https://guides.gamepressure.com/assassins-creed-odyssey",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "GamerGuides",
                    Link = new LinkGamerGuides(),
                    GameName = "Cyberpunk 2077",
                    GamePath = "cyberpunk-2077",
                    Url = "https://www.gamerguides.com/cyberpunk-2077",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "GiantBomb",
                    Link = new LinkGiantBomb(),
                    GameName = "Cyberpunk 2077",
                    GamePath = "",
                    Url = "not found!",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "Gog",
                    Link = new LibraryLinkGog(),
                    GameName = "Blade Runner",
                    GamePath = "blade_runner",
                    Url = "https://www.gog.com/en/game/blade_runner",
                    SearchedUrl = "https://www.gog.com/en/game/blade_runner"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "HG101",
                    Link = new LinkHG101(),
                    GameName = "Eternal Champions",
                    GamePath = "eternal-champions",
                    Url = "http://www.hardcoregaming101.net/eternal-champions",
                    SearchedUrl = "http://www.hardcoregaming101.net/eternal-champions/"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "HowLongToBeat",
                    Link = new LinkHowLongToBeat(),
                    GameName = "The Elder Scrolls V: Skyrim",
                    GamePath = "https://howlongtobeat.com/game/9859",
                    Url = "https://howlongtobeat.com/game/9859",
                    SearchedUrl = "https://howlongtobeat.com/game/14996"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "IGN",
                    Link = new LinkIGN(),
                    GameName = "Monkey Island 2: LeChuck's Revenge",
                    GamePath = "monkey-island-2-lechucks-revenge",
                    Url = "https://www.ign.com/games/monkey-island-2-lechucks-revenge",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "IGN Guide",
                    Link = new LinkIGNGuides(),
                    GameName = "Resident Evil 4 (Remake)",
                    GamePath = "resident-evil-4-remake",
                    Url = "https://www.ign.com/wikis/resident-evil-4-remake",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "IsThereAnyDeal",
                    Link = new LinkIsThereAnyDeal(),
                    GameName = "Outer Worlds",
                    GamePath = "outerworlds",
                    Url = "https://isthereanydeal.com/game/outerworlds",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "Itch",
                    Link = new LibraryLinkItch(),
                    GameName = "Hidden Folks",
                    GamePath = "",
                    Url = "not found!",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "LemonAmiga",
                    Link = new LinkLemonAmiga(),
                    GameName = "Indiana Jones and the Fate of Atlantis",
                    GamePath = "https://www.lemonamiga.com/games/details.php?id=558",
                    Url = "https://www.lemonamiga.com/games/details.php?id=558",
                    SearchedUrl = "https://www.lemonamiga.com/games/details.php?id=558"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "MapGenie",
                    Link = new LinkMapGenie(),
                    GameName = "Fallout 4",
                    GamePath = "fallout-4",
                    Url = "https://mapgenie.io/fallout-4",
                    SearchedUrl = "not found!"
                }
            },
            // Todo: add Metacritic -> needs Platforms!
            new object[]
            {
                new TestCase()
                {
                    CaseName = "MobyGames",
                    Link = new LinkMobyGames(),
                    GameName = "Dragon Age: Inquisition",
                    GamePath = "dragon-age-inquisition",
                    Url = "https://www.mobygames.com/game/dragon-age-inquisition",
                    SearchedUrl = "https://www.mobygames.com/game/70317/dragon-age-inquisition/"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "NECRetro",
                    Link = new LinkNECRetro(),
                    GameName = "Asuka 120% Maxima Burning Fest. Maxima",
                    GamePath = "Asuka_120%25_Maxima_Burning_Fest._Maxima",
                    Url = "https://necretro.org/Asuka_120%25_Maxima_Burning_Fest._Maxima",
                    SearchedUrl = "https://necretro.org/Asuka_120%25_Maxima_Burning_Fest._Maxima"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "NintendoWiki",
                    Link = new LinkNintendoWiki(),
                    GameName = "Castlevania: Aria of Sorrow",
                    GamePath = "Castlevania%3A_Aria_of_Sorrow",
                    Url = "https://nintendo.fandom.com/wiki/Castlevania%3A_Aria_of_Sorrow",
                    SearchedUrl = "https://nintendo.fandom.com/wiki/Castlevania:_Aria_of_Sorrow"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "PCGamingWiki",
                    Link = new LinkPCGamingWiki(),
                    GameName = "Trüberbrook",
                    GamePath = "Tr%C3%BCberbrook",
                    Url = "https://www.pcgamingwiki.com/wiki/Tr%C3%BCberbrook",
                    SearchedUrl = "https://www.pcgamingwiki.com/wiki/Tr%C3%BCberbrook_-_A_Nerd_Saves_the_World"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "RAWG",
                    Link = new LinkRAWG(),
                    GameName = "The Walking Dead: The Telltale Definitive Series",
                    GamePath = "the-walking-dead-the-telltale-definitive-series",
                    Url = "https://rawg.io/games/the-walking-dead-the-telltale-definitive-series",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "SegaRetro",
                    Link = new LinkSegaRetro(),
                    GameName = "Landstalker: The Treasures of King Nole",
                    GamePath = "Landstalker%3A_The_Treasures_of_King_Nole",
                    Url = "https://segaretro.org/Landstalker%3A_The_Treasures_of_King_Nole",
                    SearchedUrl = "https://segaretro.org/Landstalker:_The_Treasures_of_King_Nole"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "SNKWiki",
                    Link = new LinkSNKWiki(),
                    GameName = "Real Bout Fatal Fury 2: The Newcomers",
                    GamePath = "Real_Bout_Fatal_Fury_2%3A_The_Newcomers",
                    Url = "https://snk.fandom.com/wiki/Real_Bout_Fatal_Fury_2%3A_The_Newcomers",
                    SearchedUrl = "https://snk.fandom.com/wiki/Real_Bout_Fatal_Fury_2:_The_Newcomers"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "Steam",
                    Link = new LibraryLinkSteam(),
                    GameName = "Jotun: Valhalla Edition",
                    GamePath = "https://store.steampowered.com/app/323580",
                    Url = "https://store.steampowered.com/app/323580",
                    SearchedUrl = "https://store.steampowered.com/app/323580"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "StrategyWiki",
                    Link = new LinkStrategyWiki(),
                    GameName = "Killer is Dead: Nightmare Edition",
                    GamePath = "Killer_is_Dead%3A_Nightmare_Edition",
                    Url = "https://strategywiki.org/wiki/Killer_is_Dead",
                    SearchedUrl = "not found!"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "Wikipedia",
                    Link = new LinkWikipedia(),
                    GameName = "Pillars of Eternity",
                    GamePath = "Pillars_of_Eternity",
                    Url = "https://en.wikipedia.org/wiki/Pillars_of_Eternity",
                    SearchedUrl = "https://en.wikipedia.org/wiki/Pillars_of_Eternity"
                }
            },
            new object[]
            {
                new TestCase()
                {
                    CaseName = "ZopharsDomain",
                    Link = new LinkZopharsDomain(),
                    GameName = "Alien Soldier",
                    GamePath = "https://www.zophar.net/music/sega-mega-drive-genesis/alien-soldier",
                    Url = "https://www.zophar.net/music/sega-mega-drive-genesis/alien-soldier",
                    SearchedUrl = "https://www.zophar.net/music/sega-mega-drive-genesis/alien-soldier"
                }
            },
        };

        public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
