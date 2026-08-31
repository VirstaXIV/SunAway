using Dalamud.Configuration;

namespace SunAway;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    /// <summary>Master switch: remove the sun (SkyVisibility = 0) while indoors.</summary>
    public bool Enabled { get; set; } = true;

    public void Save()
        => Plugin.PluginInterface.SavePluginConfig(this);
}
