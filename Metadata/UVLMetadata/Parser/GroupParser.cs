using AngleSharp.Dom;
using KNARZhelper;
using System.Collections.Generic;
using UVLMetadata.Enums;
using UVLMetadata.Models;

namespace UVLMetadata.Parser;

public class GroupParser
{
    public List<UVLTag> Parse(IDocument groupData, TagCategoryId category)
    {
        var types = groupData.QuerySelectorAll("#main .col_container > b");
        var groupTables = groupData.QuerySelectorAll("#main .col_container > table");
        var tags = new List<UVLTag>();

        if (types.Length != groupTables.Length)
        {
            Log.Debug($"Mismatch between types and groups count: {types.Length} types, {groupTables.Length} groups.");
            return tags;
        }

        var counter = 0;

        foreach (var typeName in types)
        {
            var type = Resources.TagTypes[typeName.TextContent.Trim()];

            foreach (var group in groupTables[counter].QuerySelectorAll("tbody > tr"))
            {
                var gamecount = group.QuerySelector("td:nth-child(5)")?.TextContent.Trim();
                var gameCountNumber = gamecount?.ExtractNumber() ?? 0;

                var tag = new UVLTag
                {
                    Name = group.QuerySelector("td:nth-child(1) > a")?.TextContent.Trim() ?? string.Empty,
                    Slug = group.QuerySelector("td:nth-child(1) > a")?.GetAttribute("href") ?? string.Empty,
                    Description = group.QuerySelector("td:nth-child(2)")?.TextContent.Trim() ?? string.Empty,
                    GameCount = (int)(group.QuerySelector("td:nth-child(5)")?.TextContent.Trim().ExtractNumber() ?? 0),
                    Type = type,
                    Category = category
                };

                if (tag.Name.Replace("-", "").Trim().IsNullOrEmpty() || tag.Slug.IsNullOrEmpty() || tag.GameCount < 1)
                {
                    continue;
                }

                tags.Add(tag);
            }

            counter++;
        }

        return tags;
    }
}
