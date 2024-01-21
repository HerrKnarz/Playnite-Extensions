using KNARZhelper;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    ///     Class to add tags to games for missing links based on patterns.
    /// </summary>
    internal class TagMissingLinks : LinkAction
    {
        private static TagMissingLinks _instance;
        private static readonly object _mutex = new object();
        private TagMissingLinks() { }
        public override string ProgressMessage => "LOCLinkUtilitiesProgressTagMissingLinks";
        public override string ResultMessage => "LOCLinkUtilitiesDialogTaggedMissingLinksMessage";
        public bool TagMissingLinksAfterChange { get; set; } = false;
        public bool TagMissingLibraryLinks { get; set; } = false;

        /// <summary>
        ///     List of patterns to check for missing links
        /// </summary>
        public LinkNamePatterns MissingLinkPatterns { get; set; }

        /// <summary>
        ///     Cache for the tags so they don't have to be retrieved from the database every time.
        /// </summary>
        public Dictionary<string, Tag> TagsCache { get; set; } = new Dictionary<string, Tag>();

        public string MissingLinkPrefix { get; set; } = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");

        public static TagMissingLinks Instance()
        {
            if (_instance != null)
            {
                return _instance;
            }

            lock (_mutex)
            {
                if (_instance == null)
                {
                    _instance = new TagMissingLinks();
                }
            }

            return _instance;
        }

        /// <summary>
        ///     Retrieves a tag by its name and creates it, of none is found.
        /// </summary>
        /// <param name="key">Name of the tag</param>
        /// <returns>The found or created tag</returns>
        private Tag GetTag(string key)
        {
            key = $"{MissingLinkPrefix} {key}";

            if (TagsCache.TryGetValue(key, out Tag cachedTag))
            {
                return cachedTag;
            }

            Tag tag = API.Instance.Database.Tags.FirstOrDefault(t => t.Name == key);

            if (tag is null)
            {
                tag = new Tag(key);
                API.Instance.Database.Tags.Add(tag);
            }

            TagsCache.Add(key, tag);

            return tag;
        }

        private static bool CheckLibraryLink(Game game, string guid, string urlPattern)
        {
            LinkNamePattern pattern = new LinkNamePattern
            {
                PartialMatch = true,
                UrlPattern = urlPattern
            };

            return game.PluginId == Guid.Parse(guid) && !(game.Links?.Any(x => pattern.LinkMatch(x.Name, x.Url)) ?? false);
        }

        private bool Tag(Game game)
        {
            bool mustUpdate = false;

            foreach (LinkNamePattern pattern in MissingLinkPatterns)
            {
                Tag tag = GetTag(pattern.LinkName);

                bool isMissing = true;

                if (game.Links?.Any() ?? false)
                {
                    isMissing = !game.Links.Any(x => pattern.LinkMatch(x.Name, x.Url));
                }

                if (isMissing)
                {
                    mustUpdate |= DatabaseObjectHelper.AddDbObject(game, FieldType.Tag, tag.Id);
                }
                else
                {
                    if (game.Tags?.Any() ?? false)
                    {
                        mustUpdate |= game.TagIds.Remove(tag.Id);
                    }
                }
            }

            if (TagMissingLibraryLinks)
            {
                bool libraryTagMissing = CheckLibraryLink(game, "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e", "*gog.com*") ||
                                         CheckLibraryLink(game, "00000001-ebb2-4eec-abcb-7c89937a42bb", "*itch.io*") ||
                                         CheckLibraryLink(game, "cb91dfc9-b977-43bf-8e70-55f46e410fab", "*steampowered.com*");

                //TODO: Someday make library instances available globally to just add this function to and get the guid from them directly.

                Tag libraryTag = GetTag("Library");


                if (libraryTagMissing)
                {
                    mustUpdate |= DatabaseObjectHelper.AddDbObject(game, FieldType.Tag, libraryTag.Id);
                }
                else
                {
                    if (game.Tags?.Any() ?? false)
                    {
                        mustUpdate |= game.TagIds.Remove(libraryTag.Id);
                    }
                }
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
            => base.Execute(game, actionModifier, isBulkAction) &&
               Tag(game);
    }
}