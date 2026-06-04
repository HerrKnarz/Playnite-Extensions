using Playnite;

namespace PlayniteExtensionHelpers.GamesCommon;

public enum DoForAllTypes
{
    BlockingLoop,
    BackgroundOperation,
    SingleBlockingMultiBackground
}

public class BaseActionArgs(string id, string name, IPlayniteApi api, List<BaseActionGame> games, string pluginName)
{
    public IPlayniteApi Api { get; } = api;
    public virtual bool DebugMode { get; set; } = false;
    public virtual DoForAllTypes DoForAllType { get; set; } = DoForAllTypes.BlockingLoop;
    public virtual List<BaseActionGame> Games { get; } = games;
    public virtual bool GamesNeedUpdate { get; set; } = true;
    public virtual string Id { get; set; } = id;
    public virtual bool IsBulkAction { get; set; } = games.Count > 1;
    public virtual bool IsPausable { get; set; } = false;
    public virtual string Name { get; set; } = name;
    public virtual string PluginName { get; set; } = pluginName;
    public virtual string ProgressMessage { get; set; } = string.Empty;
    public virtual string ResultMessageId { get; set; } = string.Empty;
    public virtual bool ShowDialogs { get; set; } = true;
    public virtual bool UpdateGamesAfterLoop { get; set; } = false;
}