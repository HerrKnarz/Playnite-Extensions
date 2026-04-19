using Playnite;

namespace PlayniteCommon.GamesCommon;

public class BaseActionArgs(IPlayniteApi api)
{
    public IPlayniteApi Api { get; } = api;
    public virtual bool DebugMode { get; set; } = false;
    public virtual bool GamesNeedUpdate { get; set; } = true;
    public virtual bool IsBulkAction { get; set; } = true;
    public string PluginName { get; set; } = string.Empty;
    public virtual bool ShowDialogs { get; set; } = true;
}