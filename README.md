[![DownloadCountTotal](https://img.shields.io/github/downloads/HerrKnarz/Playnite-Extension-LinkUtilities/total?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/archive/refs/heads/master.zip)
[![LatestVersion](https://img.shields.io/github/v/release/HerrKnarz/Playnite-Extension-LinkUtilities?include_prereleases&style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/releases)
[![LastCommit](https://img.shields.io/github/last-commit/HerrKnarz/Playnite-Extension-LinkUtilities?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/commits/master)
[![Crowdin](https://badges.crowdin.net/playnite-extension-linkutiliti/localized.svg)](https://crowdin.com/project/playnite-extension-linkutiliti)
[![License](https://img.shields.io/github/license/HerrKnarz/Playnite-Extension-LinkUtilities?style=flat)](https://github.com/HerrKnarz/Playnite-Extension-LinkUtilities/blob/master/LICENSE.txt)

# Link Utilities

This is an extension for the superb open source video game library manager and launcher [Playnite](http://playnite.link/). At the moment it can do the following:

- Sort links by name via game menu or optionally automatically after metadata change.
- Add links to several websites directly by trying to find a valid link using the game name
- Search for games on several websites to add links via search dialog
- Add library links: add links to the game page of its library

The following websites are supported (and can be toggled in the settings):

| **Website**         | **Add**                                   | **Search**                | **Library**                                    |
|---------------------|-------------------------------------------|---------------------------|------------------------------------------------|
| Epic                | yes                                       | yes                       | no (can't find a way to get a definitive link) |
| GOG                 | yes                                       | yes                       | yes                                            |
| Hardcore Gaming 101 | yes                                       | yes                       | no                                             |
| itch.io             | no (link contains several variable parts) | yes                       | yes                                            |
| Lemon Amiga         | no (link doesn't contain game name)       | yes                       | no                                             |
| MetaCritic          | yes (for each platform the game has)      | no (scraping not allowed) | no                                             |
| MobyGames           | yes                                       | yes                       | no                                             |
| NEC Retro           | yes                                       | yes                       | no                                             |
| PCGamingWiki        | yes                                       | yes                       | no                                             |
| RAWG                | yes                                       | yes                       | no                                             |
| Sega Retro          | yes                                       | yes                       | no                                             |
| Steam               | no (link contains game id)                | yes                       | yes                                            |
| StrategyWiki        | yes                                       | yes                       | no                                             |
| Wikipedia           | yes                                       | yes                       | no                                             |

## Planned features
- check if links are still working, optional removal of dead links
- option to deactivate unwanted link sources in the settings
- add link to active browser window via bookmarklet (nearly done, but currently broken in playnite API)
- add links to more websites. The following are already planned:
  - How Long To Beat
  - IGDB (was included till 0.4 but had to be removed because it wasn't working as intended)
  - Nexus Mods (maybe - could replace the nexus mods add-on)
  - Is There Any Deal (maybe - don't really see many use cases, when the game already is in the library)
- (maybe) search for screenshots with configurable resolution

If you have more ideas for this extension, feel free to suggest them by opening an issue!
