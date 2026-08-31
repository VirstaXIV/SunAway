using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SunAway.Windows;

/// <summary>
/// Research window for finding which EnvState fields draw the sun: shows live
/// values and lets each float (named or unknown) be forced to zero. Flip things
/// while the sun is visible; whatever makes it vanish is the mechanism.
/// </summary>
public unsafe class LabWindow : Window
{
    private readonly EnvLab _lab;

    private record NamedRow(string Name, int[] Offsets);

    private static readonly NamedRow[] NamedRows =
    [
        new("Sun light color (0x20)", [0x20, 0x24, 0x28]),
        new("Moon light color (0x2C)", [0x2C, 0x30, 0x34]),
        new("Ambient (0x38)", [0x38, 0x3C, 0x40]),
        new("Star/constellation intensity (0x98)", [0x98, 0x9C, 0xA0, 0xA4, 0xA8]),
        new("Moon color+brightness (0xAC)", [0xAC, 0xB0, 0xB4, 0xB8, 0xBC]),
        new("Sky visibility (0xE4)", [0xE4]),
        new("Cloud colors (0x148)", [0x148, 0x14C, 0x150, 0x154, 0x158, 0x15C]),
    ];

    private record Region(string Name, int Start, int End); // [Start, End) in bytes

    private static readonly Region[] Regions =
    [
        new("Unknown 0x00C-0x020 (header gap)", 0x00C, 0x020),
        new("Unknown 0x044-0x098 (after lighting - prime sun suspect)", 0x044, 0x098),
        new("Unknown 0x0E8-0x148 (after fog)", 0x0E8, 0x148),
        new("Unknown 0x1E4-0x2F8 (tail)", 0x1E4, EnvState.Size),
    ];

    public LabWindow(EnvLab lab) : base("SunAway Env Lab###SunAwayLab")
    {
        _lab = lab;
        Size = new Vector2(430, 600);
        SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw()
    {
        var state = EnvLab.StatePtr;
        if (state == null)
        {
            ImGui.TextUnformatted("EnvManager not available (not in a zone?).");
            return;
        }

        ImGui.TextWrapped("Tick boxes to force values to 0 while watching the sun. " +
                          "Start with the prime suspect block, then narrow down by halves. " +
                          "Everything resets the moment a box is unticked.");
        ImGui.TextDisabled(_lab.HookActive
            ? "Applying via EnvStateCopy hook."
            : $"Hook unavailable ({_lab.HookError}); applying per framework tick.");

        if (ImGui.Button("Clear ALL overrides"))
            _lab.ClearAll();
        ImGui.SameLine();
        ImGui.TextDisabled($"{_lab.ZeroOffsets.Count} float(s) zeroed");

        ImGui.Separator();

        using var child = ImRaiiChild("##labscroll");
        if (!child)
            return;

        if (ImGui.CollapsingHeader("Named fields", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var row in NamedRows)
                DrawGroupRow(row.Name, row.Offsets, state);
        }

        foreach (var region in Regions)
        {
            var offsets = RegionOffsets(region).ToArray();
            var flags = region.Name.Contains("suspect") ? ImGuiTreeNodeFlags.DefaultOpen : ImGuiTreeNodeFlags.None;
            if (!ImGui.CollapsingHeader($"{region.Name}##{region.Start}", flags))
                continue;

            ImGui.PushID(region.Start);
            var allZeroed = offsets.All(_lab.IsZeroed);
            if (ImGui.SmallButton(allZeroed ? "Restore block" : "Zero whole block"))
                _lab.SetZeroed(offsets, !allZeroed);

            if (ImGui.BeginTable("##region", 3, ImGuiTableFlags.SizingFixedFit))
            {
                foreach (var offset in offsets)
                {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"0x{offset:X3}");
                    ImGui.TableNextColumn();
                    ImGui.TextDisabled($"{*(float*)((byte*)state + offset),12:0.####}");
                    ImGui.TableNextColumn();
                    var zeroed = _lab.IsZeroed(offset);
                    if (ImGui.Checkbox($"##z{offset}", ref zeroed))
                        _lab.SetZeroed(offset, zeroed);
                }

                ImGui.EndTable();
            }

            ImGui.PopID();
        }
    }

    private void DrawGroupRow(string name, int[] offsets, EnvState* state)
    {
        var zeroed = offsets.All(_lab.IsZeroed);
        if (ImGui.Checkbox($"##g{name}", ref zeroed))
            _lab.SetZeroed(offsets, zeroed);
        ImGui.SameLine();
        ImGui.TextUnformatted(name);
        ImGui.SameLine();
        ImGui.TextDisabled($"[{string.Join(", ", offsets.Select(o => (*(float*)((byte*)state + o)).ToString("0.###")))}]");
    }

    private static IEnumerable<int> RegionOffsets(Region region)
    {
        for (var offset = region.Start; offset < region.End; offset += 4)
            yield return offset;
    }

    // Minimal RAII child scope so an early-out still calls EndChild.
    private readonly struct ChildScope(bool open) : IDisposable
    {
        public static implicit operator bool(ChildScope scope) => scope._open;
        private readonly bool _open = open;
        public void Dispose() => ImGui.EndChild();
    }

    private static ChildScope ImRaiiChild(string id)
        => new(ImGui.BeginChild(id));
}
