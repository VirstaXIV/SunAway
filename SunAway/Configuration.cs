using Dalamud.Configuration;

namespace SunAway;

public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    /// <summary>Master switch: remove the sun (SkyVisibility = 0) while indoors.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>Additionally turn the glare graphics options off while indoors.</summary>
    public bool SuppressGlare { get; set; } = false;

    /// <summary>With SuppressGlare, also turn off the "glare representation" option (the Dawntrail-era extra glare pass).</summary>
    public bool AlsoDisableGlareRepresentation { get; set; } = true;

    public void Save()
        => Plugin.PluginInterface.SavePluginConfig(this);
}
