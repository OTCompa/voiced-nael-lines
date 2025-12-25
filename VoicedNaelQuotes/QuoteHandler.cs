using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VoicedNaelQuotes.Interop;

namespace VoicedNaelQuotes;

public class QuoteHandler : IDisposable
{
    public enum NaelQuote
    {
        InOut,
        InStack,
        StackOut,
        StackIn,
        SpreadOut,
        SpreadIn,
        SpreadDive,
        DiveStack,
        SpreadInStream,
        InSpreadStream,
        OutStackSpread,
        OutSpreadStack,
        InSpreadStack,
        InOutSpread,
        AddsRP1,
        AddsRP2,
    }


    // TODO: maybe figure out a way to support other languages
    // NpcYell 6492-6497, 6500-6507
    // InstanceContentTextData, 18100 18101
    public readonly Dictionary<string, NaelQuote> QuoteDict = new Dictionary<string, NaelQuote>
    {
        { "O hallowed moon,\nshine you the iron path!", NaelQuote.InOut },
        { "O hallowed moon,\ntake fire and scorch my foes!", NaelQuote.InStack },
        { "Blazing path,\nlead me to iron rule!", NaelQuote.StackOut },
        { "Take fire,\nO hallowed moon!", NaelQuote.StackIn },
        { "From on high I descend,\nthe iron path to walk!", NaelQuote.SpreadOut },
        { "From on high I descend,\nthe hallowed moon to call!", NaelQuote.SpreadIn },
        { "Fleeting light!\nAmid a rain of stars,\nexalt you the red moon!", NaelQuote.SpreadDive },
        { "Fleeting light!\n'Neath the red moon,\nscorch you the earth!", NaelQuote.DiveStack },
        { "From on high I descend,\nthe moon and stars to bring!", NaelQuote.SpreadInStream },
        { "From hallowed moon I descend,\na rain of stars to bring!", NaelQuote.InSpreadStream },
        { "Unbending iron,\ntake fire and descend!", NaelQuote.OutStackSpread },
        { "Unbending iron,\ndescend with fiery edge!", NaelQuote.OutSpreadStack },
        { "From hallowed moon I descend,\nupon burning earth to tread!", NaelQuote.InSpreadStack },
        { "From hallowed moon I bare iron,\nin my descent to wield!", NaelQuote.InOutSpread },
        { "O Bahamut! We shall stand guard as you make ready your divine judgment!", NaelQuote.AddsRP1},
        { "Ugh... None shall defy Lord Bahamut's will! On your knees, vermin!", NaelQuote.AddsRP2 },
    };

    private Plugin plugin;
    private ResourceLoader resourceLoader;
    private VfxSpawn vfxSpawn;

    public QuoteHandler(Plugin plugin, ResourceLoader resourceLoader, VfxSpawn vfxSpawn)
    {
        this.plugin = plugin;
        this.resourceLoader = resourceLoader;
        this.vfxSpawn = vfxSpawn;

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
        //if (Plugin.ClientState.TerritoryType != 733) return;
        if (type != XivChatType.NPCDialogueAnnouncements) return;

        foreach (var payload in message.Payloads)
        {
            if (payload is TextPayload { Text: not null } textPayload && IsNael(sender.ToString()))
            {
                if (QuoteDict.TryGetValue(textPayload.Text, out var quote))
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
        foreach (var obj in Plugin.ObjectTable.CharacterManagerObjects)
        {
            if (obj.BaseId == Constants.NaelBaseId)
            {
                PlayQuote(quote, obj);
                return;
            }
        }

        Plugin.Log.Debug("Did not find Nael in object table!");
    }

    public void PlayQuote(NaelQuote quote, IGameObject obj)
    {
        resourceLoader.AddFileReplacement(Constants.ScdPath, GetNaelVoicePath(quote));
        vfxSpawn.QueueActorVfx(Constants.VfxPath, obj);
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

    private bool IsNael(string name)
    {
        string[] names =
        [
            "Nael deus Darnus", // EN/DE/FR
            "ネール・デウス・ダーナス", // JP
            "奈尔·神·达纳斯" // CN
        ];

        return names.Contains(name, StringComparer.OrdinalIgnoreCase);
    }
}
