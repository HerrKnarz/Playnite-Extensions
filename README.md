[![DownloadCountTotal](https://img.shields.io/github/downloads/HerrKnarz/Playnite-Extensions/total?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/archive/refs/heads/master.zip)
[![LatestVersion](https://img.shields.io/github/v/release/HerrKnarz/Playnite-Extensions?include_prereleases&style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/releases)
[![LastCommit](https://img.shields.io/github/last-commit/HerrKnarz/Playnite-Extensions?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/commits/master)
[![Crowdin](https://badges.crowdin.net/playnite-extension-linkutiliti/localized.svg)](https://crowdin.com/project/playnite-extension-linkutiliti)
[![License](https://img.shields.io/github/license/HerrKnarz/Playnite-Extensions?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/blob/master/LICENSE.txt)

# Playnite Extensions

Thsi repository hosts all my extensions for the superb open source video game library manager and launcher [Playnite](http://playnite.link/). Only Link Utilities is released yet, but I'm working on a new extension to manage company names and have some ideas for other extensions as well, that will end up here.

If you want to use my extensions, I strongly recommend to install them via the add-ons menu in Playnite or the [official add-on database](https://playnite.link/addons.html) and not use the releases here on github, because they only consist of the add-ons that were changed lately and not all of them every time. 

If you have more ideas for these extensions, feel free to suggest them by opening an issue!

## Link Utilities

Extension to manage links for the games in the playnite library. It can do the following:

- Sort links by name or custom sort order via game menu or optionally automatically after metadata change.
- Remove unwanted links (e.g. social media links added by IGDB) via game menu or optionally automatically after metadata change.
- Remove duplicate links via game menu or optionally automatically after metadata change.
- Rename links (e.g. shorten wikipedia to wiki) via game menu or optionally automatically after metadata change.
- Add links to several websites directly by trying to find a valid link using the game name
- Search for games on several websites to add links via search dialog
- Add library links: add links to the game page of its library
- Add link to active website in your browser via bookmarklet (see [wiki](https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-URL-handler-and-bookmarklet) for more information). Works with any website.
- Add tags to games to keep track of missing links to specific websites.

To see which websites are already supported or planned to be added see the [wiki](https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-Supported-websites-for-add-&-search-function)!

### Planned features
- add links to more websites.
- check if links are still working, optional removal of dead links (postponed for now, because several sites return 403 when checked without real browser)

## Company Companion

Extension to manage game companies (see Developer and Publisher fields in Playnite). It can do the following:

- Merge duplicate companies
- Remove Business entitiy descriptions like Ltd. or Co.
- Find and merge similar named companies by removing special characters and generic strings like "Interactive", "Games", "Studios"

### Planned features
- optional fuzzy search to find similar companies
- ignore list to keep specific companies from being merged with similar ones.
- option to save merge groups to automatically merge them again, when games were added or updated.
- option to find and manually split comma separated companies.
- maybe add additional information to companies like descriptions and links (via own dialog window).

## KNARZtools

This is my playground for ideas and quick and simple dirty functions. It's tailored to my special playnite needs and won't be really useful for others. Use at your own risk!
