using System;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;

namespace SunAway;

/// <summary>
/// Removes the sun while the player is inside a housing interior by forcing the
/// environment state's SkyVisibility (EnvState + 0xE4, in the fog block; found
/// empirically 2026-08-30) to zero each frame. The write lands in a hook on the
/// game's EnvStateCopy function (same signature Ktisis uses) so it applies after
/// the game rebuilds EnvState from weather data; if the signature ever breaks,
/// it falls back to writing on every framework tick. Nothing needs restoring —
/// the game recomputes the state every frame.
/// </summary>
public sealed unsafe class SunRemover : IDisposable
{
    private const int SkyVisibilityOffset = 0xE4;

    private readonly Configuration _config;
    private volatile bool _suppress;

    public bool IsIndoors { get; private set; }
    public bool IsSunRemoved => _suppress;
    public bool HookActive => _copyHook is { IsEnabled: true };
    public string? HookError { get; private set; }

    private delegate nint EnvStateCopyDelegate(nint dest, nint src);

    [Signature("E8 ?? ?? ?? ?? 49 3B F5 75 0D", DetourName = nameof(EnvStateCopyDetour))]
    private Hook<EnvStateCopyDelegate>? _copyHook = null;

    public SunRemover(Configuration config)
    {
        _config = config;

        try
        {
            Plugin.GameInterop.InitializeFromAttributes(this);
            if (_copyHook == null)
                throw new InvalidOperationException("EnvStateCopy signature not found");
            _copyHook.Enable();
        }
        catch (Exception e)
        {
            HookError = e.Message;
            Plugin.Log.Warning($"EnvStateCopy hook unavailable ({e.Message}); falling back to per-tick writes.");
        }

        Plugin.Framework.Update += OnUpdate;
    }

    private void OnUpdate(IFramework framework)
    {
        var indoors = false;
        if (Plugin.ClientState.IsLoggedIn)
        {
            var housing = HousingManager.Instance();
            indoors = housing != null && housing->IsInside();
        }

        IsIndoors = indoors;
        _suppress = _config.Enabled && indoors;

        if (_copyHook == null && _suppress)
        {
            var env = EnvManager.Instance();
            if (env != null)
                Apply((byte*)&env->EnvState);
        }
    }

    private nint EnvStateCopyDetour(nint dest, nint src)
    {
        var ret = _copyHook!.Original(dest, src);
        if (_suppress)
            Apply((byte*)dest);
        return ret;
    }

    private static void Apply(byte* state)
        => *(float*)(state + SkyVisibilityOffset) = 0f;

    public void Dispose()
    {
        Plugin.Framework.Update -= OnUpdate;
        _copyHook?.Dispose();
    }
}
