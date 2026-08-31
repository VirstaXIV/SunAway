using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SunAway.Windows;

namespace SunAway;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInterop { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/sunaway";

    public Configuration Configuration { get; }
    public SunSuppressor Suppressor { get; }
    public EnvLab Lab { get; }

    public readonly WindowSystem WindowSystem = new("SunAway");
    private readonly ConfigWindow _configWindow;
    private readonly LabWindow _labWindow;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Lab = new EnvLab();
        Suppressor = new SunSuppressor(Configuration, Lab);

        _labWindow = new LabWindow(Lab);
        _configWindow = new ConfigWindow(Configuration, Suppressor, _labWindow);
        WindowSystem.AddWindow(_configWindow);
        WindowSystem.AddWindow(_labWindow);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open SunAway settings. \"/sunaway on|off|toggle\" switches indoor sun removal; \"/sunaway lab\" opens the environment lab.",
        });

        Log.Information("SunAway loaded.");
    }

    private void OnCommand(string command, string args)
    {
        switch (args.Trim().ToLowerInvariant())
        {
            case "on":
                SetEnabled(true);
                break;
            case "off":
                SetEnabled(false);
                break;
            case "toggle":
                SetEnabled(!Configuration.Enabled);
                break;
            case "lab":
                _labWindow.Toggle();
                break;
            default:
                ToggleConfigUi();
                break;
        }
    }

    private void SetEnabled(bool enabled)
    {
        Configuration.Enabled = enabled;
        Configuration.Save();
        ChatGui.Print($"SunAway {(enabled ? "enabled" : "disabled")}.");
    }

    private void ToggleConfigUi()
        => _configWindow.Toggle();

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;

        WindowSystem.RemoveAllWindows();
        Suppressor.Dispose();
        Lab.Dispose();
    }
}
