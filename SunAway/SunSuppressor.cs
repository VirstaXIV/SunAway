using System;
using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace SunAway;

/// <summary>
/// Watches whether the player is inside a housing interior and, while they are,
/// removes the sun by driving <see cref="EnvLab.SuppressSun"/> (SkyVisibility = 0,
/// applied by the env hook each frame). Optionally also turns off the game's
/// glare options; those are real config writes, so the player's original values
/// are captured before the first write and restored on leaving/disable/unload.
/// </summary>
public sealed class SunSuppressor : IDisposable
{
    private readonly Configuration _config;
    private readonly EnvLab _lab;

    private uint? _savedGlare;
    private uint? _savedGlareRepresentation;

    public bool IsIndoors { get; private set; }
    public bool IsSunRemoved => _lab.SuppressSun;
    public bool IsGlareSuppressed => _savedGlare.HasValue || _savedGlareRepresentation.HasValue;

    public SunSuppressor(Configuration config, EnvLab lab)
    {
        _config = config;
        _lab = lab;
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

        _lab.SuppressSun = _config.Enabled && indoors;

        var wantGlareSuppression = _config.Enabled && _config.SuppressGlare && indoors;
        if (wantGlareSuppression && !IsGlareSuppressed)
            SuppressGlare();
        else if (!wantGlareSuppression && IsGlareSuppressed)
            RestoreGlare();
    }

    private void SuppressGlare()
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
    public void RestoreGlare()
    {
        if (_savedGlare is { } glare)
            Plugin.GameConfig.Set(SystemConfigOption.Glare_DX11, glare);
        if (_savedGlareRepresentation is { } representation)
            Plugin.GameConfig.Set(SystemConfigOption.GlareRepresentation_DX11, representation);

        if (IsGlareSuppressed)
            Plugin.Log.Debug($"Glare restored to {_savedGlare}/{_savedGlareRepresentation}.");

        _savedGlare = null;
        _savedGlareRepresentation = null;
    }

    public void Dispose()
    {
        Plugin.Framework.Update -= OnUpdate;
        _lab.SuppressSun = false;
        RestoreGlare();
    }
}
