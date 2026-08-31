using System;
using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SunAway;

/// <summary>
/// Watches whether the player is inside a housing interior and, while they are,
/// zeroes the game's glare options so sunlight stops bleeding through walls.
/// The player's original values are captured before the first write and restored
/// when they leave (or when the plugin is disabled/unloaded).
/// </summary>
public sealed class SunSuppressor : IDisposable
{
    private readonly Configuration _config;

    private uint? _savedGlare;
    private uint? _savedGlareRepresentation;

    public bool IsIndoors { get; private set; }
    public bool IsSuppressed => _savedGlare.HasValue || _savedGlareRepresentation.HasValue;

    public SunSuppressor(Configuration config)
    {
        _config = config;
        Plugin.Framework.Update += OnUpdate;
    }

    private unsafe void OnUpdate(IFramework framework)
    {
        var indoors = false;
        if (Plugin.ClientState.IsLoggedIn)
        {
            var housing = HousingManager.Instance();
            indoors = housing != null && housing->IsInside();
        }

        IsIndoors = indoors;

        var wantSuppression = _config.Enabled && indoors;
        if (wantSuppression && !IsSuppressed)
            Suppress();
        else if (!wantSuppression && IsSuppressed)
            Restore();
    }

    private void Suppress()
    {
        if (Plugin.GameConfig.TryGet(SystemConfigOption.Glare_DX11, out uint glare))
        {
            _savedGlare = glare;
            Plugin.GameConfig.Set(SystemConfigOption.Glare_DX11, 0u);
        }

        if (_config.AlsoDisableGlareRepresentation
            && Plugin.GameConfig.TryGet(SystemConfigOption.GlareRepresentation_DX11, out uint representation))
        {
            _savedGlareRepresentation = representation;
            Plugin.GameConfig.Set(SystemConfigOption.GlareRepresentation_DX11, 0u);
        }

        Plugin.Log.Debug($"Indoors: glare suppressed (was {_savedGlare}/{_savedGlareRepresentation}).");
    }

    /// <summary>Restore the player's saved glare settings, if any were overridden.</summary>
    public void Restore()
    {
        if (_savedGlare is { } glare)
            Plugin.GameConfig.Set(SystemConfigOption.Glare_DX11, glare);
        if (_savedGlareRepresentation is { } representation)
            Plugin.GameConfig.Set(SystemConfigOption.GlareRepresentation_DX11, representation);

        if (IsSuppressed)
            Plugin.Log.Debug($"Glare restored to {_savedGlare}/{_savedGlareRepresentation}.");

        _savedGlare = null;
        _savedGlareRepresentation = null;
    }

    public void Dispose()
    {
        Plugin.Framework.Update -= OnUpdate;
        Restore();
    }
}
