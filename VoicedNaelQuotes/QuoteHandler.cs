using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VoicedNaelQuotes.Interop;

namespace VoicedNaelQuotes;

public partial class QuoteHandler : IDisposable
{
    private readonly Dictionary<string, NaelQuote> quoteDict = [];
    private readonly string naelName;

    private Plugin plugin;
    private ResourceLoader resourceLoader;
    private VfxSpawn vfxSpawn;

    public QuoteHandler(Plugin plugin, ResourceLoader resourceLoader, VfxSpawn vfxSpawn)
    {
        this.plugin = plugin;
        this.resourceLoader = resourceLoader;
        this.vfxSpawn = vfxSpawn;

        foreach (var quoteInfo in quoteInitInfo)
        {
            string text = quoteInfo.sheet switch
            {
                Sheet.NpcYell => Plugin.DataManager.GameData.GetExcelSheet<NpcYell>()?.GetRow(quoteInfo.rowId).Text.ToString() ?? "",
                Sheet.InstanceContentTextData => Plugin.DataManager.GameData.GetExcelSheet<InstanceContentTextData>()?.GetRow(quoteInfo.rowId).Text.ToString() ?? "",
                _ => "",
            };
            if (!string.IsNullOrEmpty(text))
            {
                quoteDict[text] = quoteInfo.quote;
            } else
            {
                Plugin.Log.Error($"Failed to initialize Quote: {quoteInfo.quote}");
            }
        }

        naelName = Plugin.DataManager.GameData.GetExcelSheet<BNpcName>()?.GetRow(naelNameRowId).Singular.ToString() ?? "";
        if (string.IsNullOrEmpty(naelName))
        {
            Plugin.Log.Error("Failed to initialize Nael name!");

        }

        Plugin.ChatGui.ChatMessage += OnChatMessage;
    }

    public void Dispose()
    {
        Plugin.ChatGui.ChatMessage -= OnChatMessage;
    }

    // adapted from https://github.com/hunter2actual/BigNaelQuotes/blob/master/BigNaelQuotes/BigNaelQuotes/BigNaelQuotes.cs
    // b38c9ca
    private void OnChatMessage(XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool isHandled)
    {
        if (Plugin.ClientState.TerritoryType != 733) return;
        if (type != XivChatType.NPCDialogueAnnouncements) return;

        foreach (var payload in message.Payloads)
        {
            if (payload is TextPayload { Text: not null } textPayload && naelName.Equals(sender.ToString()))
            {
                if (quoteDict.TryGetValue(textPayload.Text, out var quote))
                {
                    PlayNaelQuote(quote);
                    Plugin.Log.Debug($"Playing for quote {(int)quote} {quote}: {textPayload.Text}");
                } else
                {
                    Plugin.Log.Debug($"Can't find quote for: {textPayload.Text}");
                }
            }
        }
    }

    public void PlayNaelQuote(NaelQuote quote)
    {
        IGameObject? actor = null;
        if (!plugin.Configuration.DisableDirectionalAudio)
        {
            foreach (var obj in Plugin.ObjectTable.CharacterManagerObjects)
            {
                if (obj.BaseId == Constants.NaelBaseId)
                {
                    actor = obj;
                    break;
                }
            }

            if (actor == null)
            {
                Plugin.Log.Debug("Did not find Nael in object table!");
            }
        }

        PlayQuote(quote, actor);
    }

    public void PlayQuote(NaelQuote quote, IGameObject? obj)
    {
        resourceLoader.AddFileReplacement(Constants.ScdPath, GetNaelVoicePath(quote));
        if (!plugin.Configuration.DisableDirectionalAudio && obj != null)
        {
            vfxSpawn.QueueActorVfx(Constants.VfxPath, obj);
        } else
        {
            resourceLoader.PlaySound(Constants.ScdPath, 0);
        }
    }

    private string GetNaelVoicePath(NaelQuote quote)
    {
        return Path.Combine(
            Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!,
            "Resources",
            "VoiceLines",
            ((Constants.Voicepack)plugin.Configuration.Voicepack).ToString(),
            $"{(int)quote}.scd"
            );
    }
}
