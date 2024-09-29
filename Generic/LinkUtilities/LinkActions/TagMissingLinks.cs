using KNARZhelper.DatabaseObjectTypes;
using LinkUtilities.BaseClasses;
using LinkUtilities.Models;
using LinkUtilities.Settings;
using Playnite.SDK;
using Playnite.SDK.Models;
using System;
using System.Linq;

namespace LinkUtilities.LinkActions
{
    /// <summary>
    /// Class to add tags to games for missing links based on patterns.
    /// </summary>
    internal class TagMissingLinks : LinkAction
    {
        private static TagMissingLinks _instance;

        private TagMissingLinks()
        { }

        /// <summary>
        /// List of patterns to check for missing links
        /// </summary>
        public LinkNamePatterns MissingLinkPatterns { get; set; }

        public string MissingLinkPrefix { get; set; } = ResourceProvider.GetString("LOCLinkUtilitiesSettingsMissingLinkPrefixDefaultValue");
        public override string ProgressMessage => "LOCLinkUtilitiesProgressTagMissingLinks";
        public override string ResultMessage => "LOCLinkUtilitiesDialogTaggedMissingLinksMessage";
        public bool TagMissingLibraryLinks { get; set; } = false;
        public bool TagMissingLinksAfterChange { get; set; } = false;

        public static TagMissingLinks Instance() => _instance ?? (_instance = new TagMissingLinks());

        public override bool Execute(Game game, ActionModifierTypes actionModifier = ActionModifierTypes.None, bool isBulkAction = true)
                    => base.Execute(game, actionModifier, isBulkAction) &&
                       Tag(game);

        private static bool CheckLibraryLink(Game game, string guid, string urlPattern)
        {
            var pattern = new LinkNamePattern
            {
                PartialMatch = true,
                UrlPattern = urlPattern
            };

            return game.PluginId == Guid.Parse(guid) && !(game.Links?.Any(x => pattern.LinkMatch(x.Name, x.Url)) ?? false);
        }

        /// <summary>
        /// Retrieves a tag by its name and creates it, of none is found.
        /// </summary>
        /// <param name="key">Name of the tag</param>
        /// <returns>The found or created tag</returns>
        private Guid GetTag(string key) => new TypeTag().AddDbObject($"{MissingLinkPrefix} {key}");

        private bool Tag(Game game)
        {
            var mustUpdate = false;

            foreach (var pattern in MissingLinkPatterns)
            {
                var tag = GetTag(pattern.LinkName);

                var isMissing = true;

                if (game.Links?.Any() ?? false)
                {
                    isMissing = !game.Links.Any(x => pattern.LinkMatch(x.Name, x.Url));
                }

                if (isMissing)
                {
                    var typeTag = new TypeTag();

                    mustUpdate |= typeTag.AddValueToGame(game, tag);
                }
                else
                {
                    if (game.Tags?.Any() ?? false)
                    {
                        mustUpdate |= game.TagIds.Remove(tag);
                    }
                }
            }

            if (TagMissingLibraryLinks)
            {
                var libraryTagMissing = CheckLibraryLink(game, "aebe8b7c-6dc3-4a66-af31-e7375c6b5e9e", "*gog.com*") ||
                                        CheckLibraryLink(game, "00000001-ebb2-4eec-abcb-7c89937a42bb", "*itch.io*") ||
                                        CheckLibraryLink(game, "cb91dfc9-b977-43bf-8e70-55f46e410fab", "*steampowered.com*");

                //TODO: Someday make library instances available globally to just add this function to and get the guid from them directly.

                var libraryTag = GetTag("Library");

                if (libraryTagMissing)
                {
                    var typeTag = new TypeTag();

                    mustUpdate |= typeTag.AddValueToGame(game, libraryTag);
                }
                else
                {
                    if (game.Tags?.Any() ?? false)
                    {
                        mustUpdate |= game.TagIds.Remove(libraryTag);
                    }
                }
            }

            if (mustUpdate)
            {
                API.Instance.Database.Games.Update(game);
            }

            return mustUpdate;
        }
    }
}