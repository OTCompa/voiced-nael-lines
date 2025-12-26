using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace VoicedNaelQuotes.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly Plugin plugin;
    // We give this window a constant ID using ###.
    // This allows for labels to be dynamic, like "{FPS Counter}fps###XYZ counter window",
    // and the window ID will always be "###XYZ counter window" for ImGui
    public ConfigWindow(Plugin plugin) : base("Voiced Nael Quotes Config###VNQConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(400, 90);
        SizeCondition = ImGuiCond.Always;

        this.plugin = plugin;
        configuration = plugin.Configuration;
    }

    public void Dispose() { }

    private List<string> voicepacks = [
        Constants.Voicepack.Default.ToString(),
        Constants.Voicepack.Teto.ToString(),
        Constants.Voicepack.Tiktok.ToString(),
    ];

    public override void Draw()
    {
        var voicepack = configuration.Voicepack;
        if (ImGui.Combo("Voicepack", ref voicepack, voicepacks))
        {
            configuration.Voicepack = voicepack;
            configuration.Save();
        }

        ImGui.SameLine();

        if (ImGui.Button("Sample"))
        {
            plugin.SampleQuote();
        }

        var disableDirectionalAudio = configuration.DisableDirectionalAudio;
        if (ImGui.Checkbox("Disable directional audio", ref disableDirectionalAudio))
        {
            configuration.DisableDirectionalAudio = disableDirectionalAudio;
            configuration.Save();
        }
    }
}
