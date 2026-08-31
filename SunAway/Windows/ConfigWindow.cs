using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace SunAway.Windows;

public class ConfigWindow : Window
{
    private readonly Configuration _config;
    private readonly SunRemover _remover;

    public ConfigWindow(Configuration config, SunRemover remover)
        : base("SunAway###SunAwayConfig")
    {
        _config = config;
        _remover = remover;

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

        ImGui.Separator();
        ImGui.TextDisabled($"Currently: {(_remover.IsIndoors ? "indoors" : "outdoors")}, " +
                           $"sun {(_remover.IsSunRemoved ? "removed" : "untouched")}");
        if (!_remover.HookActive)
            ImGui.TextDisabled($"Hook fallback active ({_remover.HookError})");

        ImGui.Spacing();
        ImGui.TextWrapped("While you are inside a housing interior, SunAway hides the sky's " +
                          "celestial rendering (sun included) so the sun stops shining through " +
                          "the walls. Everything is back to normal the moment you step outside.");
    }
}
