using Playnite.SDK.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WikipediaMetadata.Models;
using Xunit;

namespace WikipediaMetadata.Test
{
    public class HtmlParserTest
    {
        public static TheoryData<PluginSettings, string, string> DescriptionTestData => new TheoryData<PluginSettings, string, string>
        {
            {
                new PluginSettings(),
                "The_Battle_of_Polytopia",
                "<p><i><b>The Battle of Polytopia</b></i> is a turn-based <a href=\"https://en.wikipedia.org/wiki/4X\">4X</a> strategy game developed by Swedish gaming company Midjiwan AB. Players play as one of sixteen tribes to develop an empire and defeat opponents in a <a href=\"https://en.wikipedia.org/wiki/Low_poly\">low poly</a> square-shaped world. Players can play against bots or human opponents, local or online. The game was initially released in February 2016.</p>\r\n\r\n<h2>History</h2>\r\n\r\n<p><i>The Battle of Polytopia</i> was initially released on <a href=\"https://en.wikipedia.org/wiki/IOS\">iOS</a> in February 2016 as <i><b>Super Tribes</b></i>.\nIn June 2016, the game's name was changed to <i>The Battle of Polytopia</i> due to trademark issues. The game was released on <a href=\"https://en.wikipedia.org/wiki/Android_(operating_system)\">Android</a> on December 1, 2016.\nOnline multiplayer was added on February 15, 2018. A desktop version using <a href=\"https://en.wikipedia.org/wiki/Steam_(service)\">Steam</a> was released on August 4, 2020.\nThe game was added as a feature to <a href=\"https://en.wikipedia.org/wiki/Tesla,_Inc.\">Tesla</a> cars on December 25, 2020.</p>\r\n\r\n<p>The game was released for Nintendo Switch in October of 2022.</p>\r\n\r\n<p>In 2020, a new version of the game called <i>Moonrise</i> was released. Moonrise uses the <a href=\"https://en.wikipedia.org/wiki/Unity_(game_engine)\">Unity</a> game engine, while previous versions used <a href=\"https://en.wikipedia.org/wiki/Adobe_AIR\">Adobe AIR</a>. Moonrise was released first on Steam with the desktop version's release on August 4, 2020 and then on mobile on November 23 the same year.</p>\r\n\r\n<h2>Gameplay</h2>\r\n\r\n<p><i>The Battle of Polytopia</i> can be played in single player, local multiplayer, or online game modes. Single-player and local multiplayer are free to play, while online multiplayer requires a purchase. This game has no advertisements as of now.</p>\r\n\r\n<p>Each game is played on a randomly generated map, however you can pick size and water distribution. There are multiple game modes with different win conditions and settings that customise gameplay.</p>\r\n\r\n<p>Players choose from 16 selectable characters known as tribes, of which four are free and 12 require purchase. Four of the paid tribes are special, with additional abilities and altered gameplay.</p>\r\n\r\n<p>There is a leaderboard that lists the highest-scoring games of the week.</p>\r\n\r\n<h2>Reception</h2>\r\n\r\n<p>Reviews for the mobile version of <i>The Battle of Polytopia</i> were positive. Chris Carter of <i><a href=\"https://en.wikipedia.org/wiki/TouchArcade\">TouchArcade</a></i> gave the game 4.5 stars out of 5, calling it \"a great city building game that’s free of the shackles of 'freemium.'\" The PC port of the game received \"mixed reviews\" according to <a href=\"https://en.wikipedia.org/wiki/Review_aggregator\">review aggregator</a> <a href=\"https://en.wikipedia.org/wiki/Metacritic\">Metacritic</a>. In a review for Cubed3, Eric Ace gave the game a 6 out of 10 saying, \"For some casual players it is a fun romp, but anyone looking for depth or longevity won't find it here.\"</p>\r\n\r\n<h2>Environmental initiatives</h2>\r\n\r\n<p>All revenues from sales of the game's \"Zebasi\" tribe on mobile are invested in loans for <a href=\"https://en.wikipedia.org/wiki/Solar_power\">solar power</a> projects in rural Africa with the Swedish company <a href=\"https://trine.com\">Trine</a>. Over €150,000 has been invested since October 2021.</p>\r\n\r\n<p>Midjiwan AB has donated to EARTHDAY.ORG's The Canopy Project in support of its reforestation efforts.</p>\r\n\r\n"
            },
            {
                new PluginSettings()
                {
                    RemoveDescriptionLinks = true,
                    DescriptionOverviewOnly = true,
                },
                "The_Battle_of_Polytopia",
                "<p><i><b>The Battle of Polytopia</b></i> is a turn-based 4X strategy game developed by Swedish gaming company Midjiwan AB. Players play as one of sixteen tribes to develop an empire and defeat opponents in a low poly square-shaped world. Players can play against bots or human opponents, local or online. The game was initially released in February 2016.</p>\r\n\r\n"
            },
            {
                new PluginSettings()
                {
                    RemoveDescriptionLinks = true,
                    SectionsToRemove = new ObservableCollection<string> { "Gameplay" },
                },
                "The_Battle_of_Polytopia",
                "<p><i><b>The Battle of Polytopia</b></i> is a turn-based 4X strategy game developed by Swedish gaming company Midjiwan AB. Players play as one of sixteen tribes to develop an empire and defeat opponents in a low poly square-shaped world. Players can play against bots or human opponents, local or online. The game was initially released in February 2016.</p>\r\n\r\n<h2>History</h2>\r\n\r\n<p><i>The Battle of Polytopia</i> was initially released on iOS in February 2016 as <i><b>Super Tribes</b></i>.\nIn June 2016, the game's name was changed to <i>The Battle of Polytopia</i> due to trademark issues. The game was released on Android on December 1, 2016.\nOnline multiplayer was added on February 15, 2018. A desktop version using Steam was released on August 4, 2020.\nThe game was added as a feature to Tesla cars on December 25, 2020.</p>\r\n\r\n<p>The game was released for Nintendo Switch in October of 2022.</p>\r\n\r\n<p>In 2020, a new version of the game called <i>Moonrise</i> was released. Moonrise uses the Unity game engine, while previous versions used Adobe AIR. Moonrise was released first on Steam with the desktop version's release on August 4, 2020 and then on mobile on November 23 the same year.</p>\r\n\r\n<h2>Reception</h2>\r\n\r\n<p>Reviews for the mobile version of <i>The Battle of Polytopia</i> were positive. Chris Carter of <i>TouchArcade</i> gave the game 4.5 stars out of 5, calling it \"a great city building game that’s free of the shackles of 'freemium.'\" The PC port of the game received \"mixed reviews\" according to review aggregator Metacritic. In a review for Cubed3, Eric Ace gave the game a 6 out of 10 saying, \"For some casual players it is a fun romp, but anyone looking for depth or longevity won't find it here.\"</p>\r\n\r\n<h2>Environmental initiatives</h2>\r\n\r\n<p>All revenues from sales of the game's \"Zebasi\" tribe on mobile are invested in loans for solar power projects in rural Africa with the Swedish company Trine. Over €150,000 has been invested since October 2021.</p>\r\n\r\n<p>Midjiwan AB has donated to EARTHDAY.ORG's The Canopy Project in support of its reforestation efforts.</p>\r\n\r\n"
            }
        };

        [Theory]
        [MemberData(nameof(DescriptionTestData))]
        public void TestDescription(PluginSettings settings, string gameKey, string expectedDescription)
        {
            settings.PopulateTagSettings();

            HtmlParser htmlParser = new HtmlParser(gameKey, settings);

            Assert.Equal(expectedDescription, htmlParser.Description);
        }

        public static TheoryData<PluginSettings, string, List<Link>> LinkTestData => new TheoryData<PluginSettings, string, List<Link>>
        {
            {
                new PluginSettings(),
                "The_Battle_of_Polytopia",
                new List<Link>()
                {
                    new Link()
                    {
                        Name = "Official website",
                        Url = "https://polytopia.io/",
                    }
                }
            },
            {
                new PluginSettings(),
                "Garou:_Mark_of_the_Wolves",
                new List<Link>()
                {
                    new Link()
                    {
                        Name = "Official website",
                        Url = "https://game.snk-corp.co.jp/official/online/mow/index.php",
                    },
                    new Link()
                    {
                        Name = "GameFAQs",
                        Url = "https://gamefaqs.gamespot.com/arcade/562919-garou-mark-of-the-wolves",
                    },
                    new Link()
                    {
                        Name = "Giant Bomb",
                        Url = "https://www.giantbomb.com/garou-mark-of-the-wolves/3030-10674/",
                    },
                    new Link()
                    {
                        Name = "Killer List of Videogames",
                        Url = "https://www.arcade-museum.com/game_detail.php?game_id=7918",
                    },
                    new Link()
                    {
                        Name = "MobyGames",
                        Url = "https://www.mobygames.com/game/garou-mark-of-the-wolves",
                    },
                }
            },
        };

        [Theory]
        [MemberData(nameof(LinkTestData))]
        public void TestLinks(PluginSettings settings, string gameKey, List<Link> expectedLinks)
        {
            settings.PopulateTagSettings();

            HtmlParser htmlParser = new HtmlParser(gameKey, settings);

            Assert.Equal(expectedLinks, htmlParser.Links);
        }
    }
}