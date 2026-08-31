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
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInterop { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;

    private const string CommandName = "/sunaway";

    private readonly SunRemover _remover;
    private readonly WindowSystem _windowSystem = new("SunAway");
    private readonly ConfigWindow _configWindow;

    public Plugin()
    {
        var configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        _remover = new SunRemover(configuration);

        _configWindow = new ConfigWindow(configuration, _remover);
        _windowSystem.AddWindow(_configWindow);

        PluginInterface.UiBuilder.Draw += _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += _configWindow.Toggle;
        PluginInterface.UiBuilder.OpenMainUi += _configWindow.Toggle;

        CommandManager.AddHandler(CommandName, new CommandInfo((_, _) => _configWindow.Toggle())
        {
            HelpMessage = "Open SunAway settings.",
        });
    }

    public void Dispose()
    {
        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.Draw -= _windowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= _configWindow.Toggle;
        PluginInterface.UiBuilder.OpenMainUi -= _configWindow.Toggle;

        _windowSystem.RemoveAllWindows();
        _remover.Dispose();
    }
}
