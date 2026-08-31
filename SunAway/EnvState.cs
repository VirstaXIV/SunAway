using System.Numerics;
using System.Runtime.InteropServices;

namespace SunAway;

/// <summary>
/// The environment state blended from the zone's weather envb data each frame,
/// living at EnvManager + 0x58 (matches FFXIVClientStructs' EnvState field, which
/// only maps Rain). Field layout taken from Ktisis (Ktisis/Structs/Env), which
/// maintains these offsets for its GPose environment editor.
/// Everything not named here is unknown territory — the Env Lab window exposes
/// those bytes as raw floats so the sun-disk fields can be found empirically.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = Size)]
public struct EnvState
{
    public const int Size = 0x2F8;

    [FieldOffset(0x008)] public uint SkyId;

    // Lighting block (0x20..0x60)
    [FieldOffset(0x020)] public Vector3 SunLightColor;
    [FieldOffset(0x02C)] public Vector3 MoonLightColor;
    [FieldOffset(0x038)] public Vector3 Ambient;
    [FieldOffset(0x048)] public float AmbientSaturation;
    [FieldOffset(0x04C)] public float Temperature;

    // Stars block (0x98..0xC0)
    [FieldOffset(0x098)] public float ConstellationIntensity;
    [FieldOffset(0x09C)] public float Constellations;
    [FieldOffset(0x0A0)] public float Stars;
    [FieldOffset(0x0A4)] public float GalaxyIntensity;
    [FieldOffset(0x0A8)] public float StarIntensity;
    [FieldOffset(0x0AC)] public Vector4 MoonColor;
    [FieldOffset(0x0BC)] public float MoonBrightness;

    // Fog block (0xC0..0xE8)
    [FieldOffset(0x0C0)] public Vector4 FogColor;
    [FieldOffset(0x0D0)] public float FogDistance;
    [FieldOffset(0x0D4)] public float FogThickness;
    [FieldOffset(0x0E0)] public float FogOpacity;
    [FieldOffset(0x0E4)] public float SkyVisibility;

    // Clouds block (0x148..0x170)
    [FieldOffset(0x148)] public Vector3 CloudColor;
    [FieldOffset(0x168)] public uint CloudTexture;
    [FieldOffset(0x16C)] public uint CloudSideTexture;

    // Rain 0x170..0x1A4, Dust 0x1A4..0x1D8, Wind 0x1D8..0x1E4 (unused here)
}
