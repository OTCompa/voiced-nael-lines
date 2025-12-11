using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.IO;
using VoicedNaelLines.Interop;
using VoicedNaelLines.Windows;

namespace VoicedNaelLines;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider {  get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("VoicedNaelLines");
    private ConfigWindow ConfigWindow { get; init; }
    private VfxSpawn VfxSpawn { get; init; }
    private ResourceLoader ResourceLoader { get; init; }
    private QuoteHandler QuoteHandler { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

#if DEBUG
        InitDebug();
#endif

        // Tell the UI system that we want our windows to be drawn throught he window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        ResourceLoader = new ResourceLoader();
        VfxSpawn = new VfxSpawn(ResourceLoader);
        QuoteHandler = new QuoteHandler(this, ResourceLoader, VfxSpawn);

        if (ClientState.TerritoryType == Constants.UCoBTerritoryId)
        {
            ResourceLoader.AddFileReplacement(Constants.VfxPath, Utility.GetResourcePath(PluginInterface, Constants.LocalVfxFilename));
        }

        ClientState.TerritoryChanged += OnTerritoryChanged;
    }

    public void Dispose()
    {
        QuoteHandler.Dispose();
        VfxSpawn.Dispose();
        ResourceLoader.RemoveFileReplacement(Constants.VfxPath);
        ClientState.TerritoryChanged -= OnTerritoryChanged;

        // Unregister all actions to not leak anythign during disposal of plugin
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
#if DEBUG
        DisposeDebug();
#endif
    }

    private void OnTerritoryChanged(ushort obj)
    {
        if (obj == Constants.UCoBTerritoryId)
        {
            ResourceLoader.AddFileReplacement(Constants.VfxPath, Utility.GetResourcePath(PluginInterface, Constants.LocalVfxFilename));
        }
        else
        {
            ResourceLoader.RemoveFileReplacement(Constants.VfxPath);
        }
    }

#if DEBUG
    private const string TestCommand = "/pvnltest";
    private void InitDebug()
    {
        CommandManager.AddHandler(TestCommand, new CommandInfo(OnTest)
        {
            HelpMessage = "A useful message to display in /xlhelp"
        });
    }
    private void DisposeDebug()
    {
        CommandManager.RemoveHandler(TestCommand);
    }
    private void OnTest(string command, string args)
    {
        var test = int.Parse(args);
        if (ObjectTable.LocalPlayer == null) return; 
        var target = ObjectTable.LocalPlayer.TargetObject;

        ResourceLoader.AddFileReplacement(Constants.VfxPath, Utility.GetResourcePath(PluginInterface, Constants.LocalVfxFilename));

        if (target != null)
        {
            QuoteHandler.PlayQuote((QuoteHandler.NaelQuote)test, target);
        } else
        {
            QuoteHandler.PlayQuote((QuoteHandler.NaelQuote)test, ObjectTable.LocalPlayer);
        }
    }
#endif

    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
