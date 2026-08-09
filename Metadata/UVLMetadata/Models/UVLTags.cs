using KNARZhelper.FilesCommon;
using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.IO;

namespace UVLMetadata.Models;

public class UVLTags(UVLMetadata plugin) : List<UVLTag>
{
    private readonly string _fileName = Path.Combine(plugin.GetPluginUserDataPath(), "UVLTags.json");

    [DontSerialize]
    public DateTime LastRefresh => File.Exists(_fileName) ? File.GetLastWriteTime(_fileName) : DateTime.MinValue;

    public Dictionary<string, UVLTag> GetTagDictionary()
    {
        var tagDictionary = new Dictionary<string, UVLTag>();

        foreach (var tag in this)
        {
            if (!tagDictionary.ContainsKey(tag.Slug))
            {
                tagDictionary.Add(tag.Slug, tag);
            }
        }

        return tagDictionary;
    }

    /// <summary>
    /// Loads the tags from a JSON file with the same structure.
    /// </summary>
    /// <param name="file">The JSON file to read from.</param>
    public void LoadFromFile()
    {
        Clear();

        var file = new FileInfo(_fileName);

        if (!file.Exists)
        {
            return;
        }

        var tags = Serialization.FromJsonFile<List<UVLTag>>(file.FullName);

        AddRange(tags);
    }

    /// <summary>
    /// Saves the tags to a JSON file.
    /// </summary>
    public void Save()
    {
        if (string.IsNullOrEmpty(_fileName))
        {
            return;
        }

        var serializedData = Serialization.ToJson(this, true);

        FileHelper.WriteStringToFile(_fileName, serializedData, true);
    }
}
