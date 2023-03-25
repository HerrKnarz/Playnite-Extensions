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
    /// Class to add tags to games for missing links based on patterns.
    /// </summary>
    internal class TagMissingLinks : BaseClasses.LinkAction
    {
        private static TagMissingLinks _instance = null;
        private static readonly object _mutex = new object();
        private TagMissingLinks() : base()
        {
        }

        public static TagMissingLinks Instance()
        {
            if (_instance == null)
            {
                lock (_mutex)
                {
                    if (_instance == null)
                    {
                        _instance = new TagMissingLinks();
                    }
                }
            }

            return _instance;
        }

        public override string ProgressMessage { get; } = "LOCLinkUtilitiesProgressTagMissingLinks";

        public override string ResultMessage { get; } = "LOCLinkUtilitiesDialogTaggedMissingLinksMessage";

        public bool TagMissingLinksAfterChange { get; set; } = false;

        /// <summary>
        /// List of patterns to check for missing links
        /// </summary>
        public LinkNamePatterns MissingLinkPatterns { get; set; }

        /// <summary>
        /// Cache for the tags so they don't have to be retrieved from the database every time.
        /// </summary>
        public Dictionary<string, Tag> TagsCache { get; set; } = new Dictionary<string, Tag>();

        public string MissingLinkPrefix { get; set; } = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");

        /// <summary>
        /// Retrieves a tag by its name and creates it, of none is found.
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

        /// <summary>
        /// Adds a tag to a game.
        /// </summary>
        /// <param name="game">Game the tag will be added to</param>
        /// <param name="tag">The tag to add.</param>
        /// <returns>True if the tag was added</returns>
        private bool AddTagToGame(Game game, Tag tag)
        {
            List<Guid> tagIds = game.TagIds ?? (game.TagIds = new List<Guid>());

            if (!tagIds.Contains(tag.Id))
            {
                tagIds.Add(tag.Id);
                return true;
            }

            return false;
        }

        private bool Tag(Game game)
        {
            bool mustUpdate = false;

            foreach (LinkNamePattern pattern in MissingLinkPatterns)
            {
                Tag tag = GetTag(pattern.LinkName);

                bool isMissing = true;

                if (game.Links != null && game.Links.Count > 0)
                {
                    isMissing = game.Links.Where(x => pattern.LinkMatch(x.Name, x.Url)).Count() == 0;
                }

                if (isMissing)
                {
                    mustUpdate |= AddTagToGame(game, tag);
                }
                else
                {
                    if (game.Tags != null && game.Tags.Count > 0)
                    {
                        mustUpdate |= game.TagIds.Remove(tag.Id);
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
            => Tag(game);
    }
}
