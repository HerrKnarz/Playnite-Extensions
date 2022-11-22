[![DownloadCountTotal](https://img.shields.io/github/downloads/HerrKnarz/Playnite-Extension-LinkUtilities/total?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/archive/refs/heads/master.zip)
[![LatestVersion](https://img.shields.io/github/v/release/HerrKnarz/Playnite-Extension-LinkUtilities?include_prereleases&style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/releases)
[![LastCommit](https://img.shields.io/github/last-commit/HerrKnarz/Playnite-Extension-LinkUtilities?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/commits/master)
[![Crowdin](https://badges.crowdin.net/playnite-extension-linkutiliti/localized.svg)](https://crowdin.com/project/playnite-extension-linkutiliti)
[![License](https://img.shields.io/github/license/HerrKnarz/Playnite-Extension-LinkUtilities?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/blob/master/LICENSE.txt)

# Link Utilities

This is an extension for the superb open source video game library manager and launcher [Playnite](http://playnite.link/). At the moment it can do the following:

- Sort links by name or custom sort order via game menu or optionally automatically after metadata change.
- Remove unwanted links (e.g. social media links added by IGDB) via game menu or optionally automatically after metadata change.
- Rename links (e.g. shorten wikipedia to wiki) via game menu or optionally automatically after metadata change.
- Add links to several websites directly by trying to find a valid link using the game name
- Search for games on several websites to add links via search dialog
- Add library links: add links to the game page of its library
- Add link to active website in your browser via bookmarklet (see [wiki](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/wiki/URL-handler-and-bookmarklet#bookmarklet) for more information). Works with any website.


The following websites are supported for add/search functions (and can be toggled in the settings):

| **Website**         | **Add**                                               | **Search**                | **Library**                                    |
|---------------------|-------------------------------------------------------|---------------------------|------------------------------------------------|
| Arcade Database     | yes (only for game roms following MAME naming scheme) | yes                       | no                                             |
| Co-Optimus          | no (link contains several variable parts)             | yes                       | no                                             |
| Epic                | yes                                                   | yes                       | no (can't find a way to get a definitive link) |
| Gamer Guides        | yes                                                   | no (scraping not allowed) | no                                             |
| GOG                 | yes                                                   | yes                       | yes                                            |
| Hardcore Gaming 101 | yes                                                   | yes                       | no                                             |
| itch.io             | no (link contains several variable parts)             | yes                       | yes                                            |
| Lemon Amiga         | no (link doesn't contain game name)                   | yes                       | no                                             |
| MetaCritic          | yes (for each platform the game has)                  | no (scraping not allowed) | no                                             |
| MobyGames           | yes                                                   | yes                       | no                                             |
| NEC Retro           | yes                                                   | yes                       | no                                             |
| PCGamingWiki        | yes                                                   | yes                       | no                                             |
| RAWG                | yes                                                   | yes                       | no                                             |
| Sega Retro          | yes                                                   | yes                       | no                                             |
| Steam               | no (link contains game id)                            | yes                       | yes                                            |
| StrategyWiki        | yes                                                   | yes                       | no                                             |
| Wikipedia           | yes                                                   | yes                       | no                                             |

## Planned features
- add links to more websites. The following are already planned:
  - Gamepressure Guides
  - How Long To Beat
  - IGDB (was included till 0.4 but had to be removed because it wasn't working as intended)
  - IGN
  - IGN Guides
  - MAME Smart Database (maybe not, because the site can't be scraped and returns 200 even on non valid links - could only work with a list of mame rom names)
  - Nexus Mods (maybe - could replace the nexus mods add-on)
  - Nintendo Fandom
  - OpenCritic
  - SNK Wiki
  - Zophar's Domain (Music)
- check if links are still working, optional removal of dead links (postponed for now, because several sites return 403 when checked without real browser)

### Websites I won't add to the extension:
- Arcade History: They don't have an URL based search function, no API and the game links contain variable parts unknown to Playnite.
- GameFAQs: They don't allow scraping, have no public API and the game links contain variable parts unknown to Playnite.

If you have more ideas for this extension, feel free to suggest them by opening an issue!
