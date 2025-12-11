using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace VoicedNaelQuotes.Interop;

public unsafe partial class ResourceLoader : IDisposable
{
    public const string ActorVfxCreateSig = "40 53 55 56 57 48 81 EC ?? ?? ?? ?? 0F 29 B4 24 ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 0F B6 AC 24 ?? ?? ?? ?? 0F 28 F3 49 8B F8";
    public const string ActorVfxRemoveSig = "0F 11 48 10 48 8D 05";

    public const string ReadFileSig = "48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B 05 ?? ?? ?? ?? 48 33 C4 48 89 84 24 ?? ?? ?? ?? 48 63 42";
    public const string GetResourceSyncSig = "E8 ?? ?? ?? ?? 48 8B C8 8B C3 F0 0F C0 81";
    public const string GetResourceAsyncSig = "E8 ?? ?? ?? 00 48 8B D8 EB ?? F0 FF 83 ?? ?? 00 00";
    public const string ReadSqPackSig = "40 56 41 56 48 83 EC ?? 0F BE 02";

    public const string LodConfigSig = "48 8B 05 ?? ?? ?? ?? B3";

    public const string CheckFileStateSig = "E8 ?? ?? ?? ?? 48 85 C0 74 ?? 4C 8B C8 ";

    public const string TexResourceHandleOnLoadSig = "40 53 55 41 54 41 55 41 56 41 57 48 81 EC ?? ?? ?? ?? 48 8B D9";

    public const string LoadScdLocalSig = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 30 8B 79 ?? 48 8B DA 8B D7";
    public const string SoundOnLoadSig = "40 56 57 41 54 48 81 EC 90 00 00 00 80 3A 0B 45 0F B6 E0 48 8B F2";
    public const string PlaySoundSig = "E8 ?? ?? ?? ?? E9 ?? ?? ?? ?? FE C2";
    public const string InitSoundSig = "E8 ?? ?? ?? ?? 8B 5D 77";

    public VfxSpawn? vfxSpawn;

    public ResourceLoader()
    {
        var hooks = Plugin.GameInteropProvider;
        var sigScanner = Plugin.SigScanner;

        hooks.InitializeFromAttributes(this);

        // Replace

        ReadSqPackHook = hooks.HookFromSignature<ReadSqPackPrototype>(ReadSqPackSig, ReadSqPackDetour);
        GetResourceSyncHook = hooks.HookFromSignature<GetResourceSyncPrototype>(GetResourceSyncSig, GetResourceSyncDetour);
        GetResourceAsyncHook = hooks.HookFromSignature<GetResourceAsyncPrototype>(GetResourceAsyncSig, GetResourceAsyncDetour);
        ReadFile = Marshal.GetDelegateForFunctionPointer<ReadFilePrototype>(sigScanner.ScanText(ReadFileSig));

        ReadSqPackHook.Enable();
        GetResourceSyncHook.Enable();
        GetResourceAsyncHook.Enable();

        // VFX

        var actorVfxCreateAddress = sigScanner.ScanText(ActorVfxCreateSig);
        ActorVfxCreate = Marshal.GetDelegateForFunctionPointer<ActorVfxCreateDelegate>(actorVfxCreateAddress);

        var actorVfxRemoveAddressTemp = sigScanner.ScanText(ActorVfxRemoveSig) + 7;
        var actorVfxRemoveAddress = Marshal.ReadIntPtr(actorVfxRemoveAddressTemp + Marshal.ReadInt32(actorVfxRemoveAddressTemp) + 4);
        ActorVfxRemove = Marshal.GetDelegateForFunctionPointer<ActorVfxRemoveDelegate>(actorVfxRemoveAddress);
        ActorVfxRemoveHook = hooks.HookFromAddress<ActorVfxRemoveDelegate>(actorVfxRemoveAddress, ActorVfxRemoveDetour);

        ActorVfxRemoveHook.Enable();

        // Crc

        CheckFileStateHook = hooks.HookFromSignature<CheckFileStatePrototype>(CheckFileStateSig, CheckFileStateDetour);

        CheckFileStateHook.Enable();
        SoundOnLoadHook.Enable();

        PathResolved += AddCrc;

        // Sound
        PlaySoundPath = Marshal.GetDelegateForFunctionPointer<PlaySoundDelegate>(sigScanner.ScanText(PlaySoundSig));
        InitSoundHook = hooks.HookFromSignature<InitSoundPrototype>(InitSoundSig, InitSoundDetour);


        InitSoundHook.Enable();
    }

    public void Dispose()
    {
        ReadSqPackHook.Dispose();
        GetResourceSyncHook.Dispose();
        GetResourceAsyncHook.Dispose();

        ActorVfxRemoveHook.Dispose();

        CheckFileStateHook.Dispose();
        SoundOnLoadHook.Dispose();

        PathResolved -= AddCrc;

    }
}
