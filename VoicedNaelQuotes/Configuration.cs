using Dalamud.Configuration;
using System;

namespace VoicedNaelQuotes;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public int Voicepack = 0;
    public bool DisableDirectionalAudio = false;

    // The below exist just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
