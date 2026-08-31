using Dalamud.Configuration;

namespace SunAway;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    /// <summary>Master switch: suppress glare while indoors.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Also turn off the "glare representation" option (the Dawntrail-era extra glare pass).</summary>
    public bool AlsoDisableGlareRepresentation { get; set; } = true;

    public void Save()
        => Plugin.PluginInterface.SavePluginConfig(this);
}
