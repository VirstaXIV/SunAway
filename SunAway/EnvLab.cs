using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;

namespace SunAway;

/// <summary>
/// Forces chosen EnvState float offsets to zero every frame, for hunting down
/// which fields draw the sun. Preferred mechanism is the same EnvStateCopy hook
/// Ktisis uses (the game recomputes EnvState from weather data each frame, so
/// overrides must be applied after that copy); if the signature breaks, falls
/// back to writing on every framework tick, which usually still wins the frame.
/// </summary>
public sealed unsafe class EnvLab : IDisposable
{
    private readonly HashSet<int> _zeroOffsets = [];
    private int[] _snapshot = [];

    public string? HookError { get; private set; }
    public bool HookActive => _copyHook is { IsEnabled: true };
    public IReadOnlyCollection<int> ZeroOffsets => _zeroOffsets;

    private delegate nint EnvStateCopyDelegate(nint dest, nint src);

    [Signature("E8 ?? ?? ?? ?? 49 3B F5 75 0D", DetourName = nameof(EnvStateCopyDetour))]
    private Hook<EnvStateCopyDelegate>? _copyHook = null;

    public EnvLab()
    {
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
            Plugin.Framework.Update += OnUpdate;
        }
    }

    public static EnvState* StatePtr
    {
        get
        {
            var env = EnvManager.Instance();
            return env == null ? null : (EnvState*)&env->EnvState;
        }
    }

    public bool IsZeroed(int offset)
        => _zeroOffsets.Contains(offset);

    public void SetZeroed(int offset, bool zeroed)
    {
        if (zeroed) _zeroOffsets.Add(offset);
        else _zeroOffsets.Remove(offset);
        _snapshot = _zeroOffsets.ToArray();
    }

    public void SetZeroed(IEnumerable<int> offsets, bool zeroed)
    {
        foreach (var offset in offsets)
        {
            if (zeroed) _zeroOffsets.Add(offset);
            else _zeroOffsets.Remove(offset);
        }

        _snapshot = _zeroOffsets.ToArray();
    }

    public void ClearAll()
    {
        _zeroOffsets.Clear();
        _snapshot = [];
    }

    private nint EnvStateCopyDetour(nint dest, nint src)
    {
        var ret = _copyHook!.Original(dest, src);
        Apply((byte*)dest);
        return ret;
    }

    private void OnUpdate(IFramework framework)
    {
        var state = StatePtr;
        if (state != null)
            Apply((byte*)state);
    }

    private void Apply(byte* state)
    {
        // _snapshot is an immutable array swapped on change, so the render-thread
        // detour never races the UI thread mutating the set.
        var offsets = _snapshot;
        foreach (var offset in offsets)
            *(float*)(state + offset) = 0f;
    }

    public void Dispose()
    {
        Plugin.Framework.Update -= OnUpdate;
        _copyHook?.Dispose();
    }
}
