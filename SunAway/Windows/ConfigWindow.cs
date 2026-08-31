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

        var alsoRepresentation = _config.AlsoDisableGlareRepresentation;
        if (ImGui.Checkbox("Also disable glare representation", ref alsoRepresentation))
        {
            _config.AlsoDisableGlareRepresentation = alsoRepresentation;
            _config.Save();
            if (_suppressor.IsSuppressed)
            {
                // Re-apply so the new choice takes effect immediately.
                _suppressor.Restore();
            }
        }

        ImGui.Separator();
        ImGui.TextDisabled($"Currently: {(_suppressor.IsIndoors ? "indoors" : "outdoors")}, " +
                           $"glare {(_suppressor.IsSuppressed ? "suppressed" : "untouched")}");

        ImGui.Spacing();
        ImGui.TextWrapped("While you are inside a housing interior, SunAway turns off the game's " +
                          "glare so the sun stops shining through the walls. Your graphics settings " +
                          "are restored as soon as you step outside.");

        ImGui.Separator();
        if (ImGui.Button("Open Env Lab"))
            _labWindow.Toggle();
        ImGui.SameLine();
        ImGui.TextDisabled("find what actually draws the sun");
    }
}
