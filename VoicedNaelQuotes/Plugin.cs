using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System;
using System.IO;
using VoicedNaelQuotes.Interop;
using VoicedNaelQuotes.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace VoicedNaelQuotes;

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

    public readonly WindowSystem WindowSystem = new("VoicedNaelQuotes");
    private ConfigWindow ConfigWindow { get; init; }
    private VfxSpawn VfxSpawn { get; init; }
    private ResourceLoader ResourceLoader { get; init; }
    private QuoteHandler QuoteHandler { get; init; }
    private Random random {  get; init; }
    private const string ConfigCommand = "/pvnqconfig";

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        random = new Random();

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(ConfigCommand, new CommandInfo(ToggleConfigUi)
        {
            HelpMessage = "Toggles the configuration window for Voiced Nael Quotes."
        });

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

        CommandManager.RemoveHandler(ConfigCommand);

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
    private const string TestCommand = "/pvnqtest";
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
        var arg = int.Parse(args);
        if (arg >= Enum.GetNames<QuoteHandler.NaelQuote>().Length) return;
        PlayQuote((QuoteHandler.NaelQuote)arg);

        if (arg < 14)
        {
            var npcYell = DataManager.GameData.GetExcelSheet<NpcYell>();
            if (npcYell == null) return;
            var quoteNum = arg > 5 ? arg + 2 : arg;
            var rawQuote = npcYell.GetRow((uint)(6492 + quoteNum));
            ChatGui.Print($"{rawQuote.Text}");
        } else
        {
            var instanceContentTextData = DataManager.GameData.GetExcelSheet<InstanceContentTextData>();
            if (instanceContentTextData == null) return;
            var rawQuote = instanceContentTextData.GetRow((uint)(18100 + arg - 14));
            ChatGui.Print($"{rawQuote.Text}");
        }
    }
#endif

    public void SampleQuote()
    {
        PlayQuote((QuoteHandler.NaelQuote)random.Next(Enum.GetNames<QuoteHandler.NaelQuote>().Length));
    }

    private void PlayQuote(QuoteHandler.NaelQuote quote)
    {
        if (ObjectTable.LocalPlayer == null) return;
        var target = ObjectTable.LocalPlayer.TargetObject;

        ResourceLoader.AddFileReplacement(Constants.VfxPath, Utility.GetResourcePath(PluginInterface, Constants.LocalVfxFilename));

        if (target != null)
        {
            QuoteHandler.PlayQuote(quote, target);
        }
        else
        {
            QuoteHandler.PlayQuote(quote, null);
        }
    }

    public void ToggleConfigUi(string command, string args) => ConfigWindow.Toggle();
    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
