[![DownloadCountTotal](https://img.shields.io/github/downloads/HerrKnarz/Playnite-Extensions/total?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/archive/refs/heads/master.zip)
[![LatestVersion](https://img.shields.io/github/v/release/HerrKnarz/Playnite-Extensions?include_prereleases&style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/releases)
[![LastCommit](https://img.shields.io/github/last-commit/HerrKnarz/Playnite-Extensions?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/commits/master)
[![Crowdin](https://badges.crowdin.net/playnite-extension-linkutiliti/localized.svg)](https://crowdin.com/project/playnite-extension-linkutiliti)
[![License](https://img.shields.io/github/license/HerrKnarz/Playnite-Extensions?style=flat)](https://github.com/HerrKnarz/Playnite-Extensions/blob/master/LICENSE.txt)

# Playnite Extensions

This repository hosts all my extensions for the superb open source video game library manager and launcher [Playnite](http://playnite.link/).

If you want to use my extensions, I strongly recommend to install them via the add-ons menu in Playnite or the [official add-on database](https://playnite.link/addons.html) and not use the releases here on github, because they only consist of the add-ons that were changed lately and not all of them every time. 

If you have more ideas for these extensions, feel free to suggest them by opening an issue!

## Wikipedia Metadata

Extension to get metadata from wikipedia. The following fields are supported:

- Name
- Release Date
- Genres
- Developers
- Publishers
- Features
- Tags (arcade system, engine, director, producer, designer, programmer, artist, writer, composer) Each can be toggled!
- Links
- Series
- Platform
- Cover image
- Critic Score (Metacritic and GameRankings as fallback)
- Description (with option to leave out certain sections)

### Planned features
- Maybe add wikipedia categories as optional tags (metadata addons can't directly fill the categories field).

## Link Utilities

Extension to manage links for the games in the playnite library. It can do the following:

- Sort links by name or custom sort order.
- Remove unwanted links (e.g. social media links added by IGDB).
- Remove duplicate links.
- Rename links (e.g. shorten wikipedia to wiki).
- Add links to several websites directly by trying to find a valid link using the game name
- limited option to add your own websites to be added. See [wiki](https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-Supported-websites-for-add-&-search-function) for details.
- Add library links: add links to the game page of its library
- Add tags to games to keep track of missing links to specific websites.

All of these functions can be triggered manually for the selected games via game or playnite menu or configured to be triggered automatically after the metadata of a game changes.

- Search for a game on several websites to add links via search dialog
- Open a search for a game in your web browser on supported websites.
- Add link to active website in your browser via bookmarklet (see [wiki](https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-URL-handler-and-bookmarklet) for more information). Works with any website.
- Check if links are still working, with option to filter results, remove links or replace URLs in the result dialog.

To see which websites are already supported or planned to be added see the [wiki](https://github.com/HerrKnarz/Playnite-Extensions/wiki/Link-Utilities:-Supported-websites-for-add-&-search-function)!

### Planned features
- add links to more websites.
- blacklist games from adding "missing link" tags for specific websites.
- add support for quick search to add links or search for games with specific links
- option to remove links to manually added url mask from selected games 

## Company Companion

Extension to manage game companies (see Developer and Publisher fields in Playnite). It can do the following:

- Merge duplicate companies
- Remove Business entity descriptions like Ltd. or Co.
- Find and merge similar named companies by removing special characters and generic strings like "Interactive", "Games", "Studios"

### Planned features
- optional fuzzy search to find similar companies
- option to filter only publishers or developers, to manage then separately.
- ignore list to keep specific companies from being merged with similar ones.
- option to save merge groups to automatically merge them again, when games were added or updated.
- option to find and manually split comma separated companies.
- maybe add additional information to companies like descriptions and links (via own dialog window).

## Quick Add

Extension to quickly add, remove or toggle categories, features and tags in games through the game context menu instead of going through the edit dialog.

- You can configure which will show up in the menu via the addon settings. Per default non are shown.
- An icon left of the values in the menu indicates, if it is already set in all selected games (check mark in a circle), some of the selectec games (check marl withot circle) or in none (cross in circle).

### Planned features
- custom menu entry path for each value to make it possible to group them.
- option to show all sub menus under one "Quick Add" menu entry or one for each type and action on the top level (which is the default at the moment).

## KNARZtools

This is my playground for ideas and quick and simple dirty functions. It's tailored to my special playnite needs and won't be really useful for others. Use at your own risk!
