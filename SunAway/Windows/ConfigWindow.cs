using System;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SunAway.Windows;

public class ConfigWindow : Window
{
    private readonly Configuration _config;
    private readonly SunSuppressor _suppressor;
    private readonly LabWindow _labWindow;

    public ConfigWindow(Configuration config, SunSuppressor suppressor, LabWindow labWindow)
        : base("SunAway###SunAwayConfig")
    {
        _config = config;
        _suppressor = suppressor;
        _labWindow = labWindow;

        Flags = ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void Draw()
    {
        var enabled = _config.Enabled;
        if (ImGui.Checkbox("Remove the sun while indoors", ref enabled))
        {
            _config.Enabled = enabled;
            _config.Save();
        }

        var suppressGlare = _config.SuppressGlare;
        if (ImGui.Checkbox("Also turn off glare while indoors", ref suppressGlare))
        {
            _config.SuppressGlare = suppressGlare;
            _config.Save();
        }

        if (_config.SuppressGlare)
        {
            ImGui.Indent();
            var alsoRepresentation = _config.AlsoDisableGlareRepresentation;
            if (ImGui.Checkbox("Include glare representation", ref alsoRepresentation))
            {
                _config.AlsoDisableGlareRepresentation = alsoRepresentation;
                _config.Save();
                // Re-apply so the new choice takes effect immediately.
                _suppressor.RestoreGlare();
            }

            ImGui.Unindent();
        }

        ImGui.Separator();
        ImGui.TextDisabled($"Currently: {(_suppressor.IsIndoors ? "indoors" : "outdoors")}, " +
                           $"sun {(_suppressor.IsSunRemoved ? "removed" : "untouched")}" +
                           (_suppressor.IsGlareSuppressed ? ", glare suppressed" : ""));

        ImGui.Spacing();
        ImGui.TextWrapped("While you are inside a housing interior, SunAway hides the sky's " +
                          "celestial rendering (sun included) so the sun stops shining through " +
                          "the walls. Everything is back to normal the moment you step outside.");

        ImGui.Separator();
        if (ImGui.Button("Open Env Lab"))
            _labWindow.Toggle();
        ImGui.SameLine();
        ImGui.TextDisabled("poke the environment state directly");
    }
}
