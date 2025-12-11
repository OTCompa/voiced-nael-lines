// Adapted from https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/ResourceLoader.Crc.cs
// 855ac66
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using Penumbra.String.Classes;
using VoicedNaelQuotes.Interop.Structs;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading;

namespace VoicedNaelQuotes.Interop;

public unsafe partial class ResourceLoader
{
    public static readonly IntPtr CustomFileFlag = new(0xDEADBEEE);

    private readonly HashSet<ulong> CustomMdlCrc = [];
    private readonly HashSet<ulong> CustomTexCrc = [];
    private readonly HashSet<ulong> CustomScdCrc = [];

    private readonly ThreadLocal<bool> TexReturnData = new(() => default);
    private readonly ThreadLocal<bool> ScdReturnData = new(() => default);

    // ====== LOD ==========

    [Signature(LodConfigSig)]
    private readonly nint LodConfig = nint.Zero;

    public byte GetLod(TextureResourceHandle* handle)
    {
        if (handle->ChangeLod)
        {
            var config = *(byte*)LodConfig + 0xE;
            if (config == byte.MaxValue) return 2;
        }
        return 0;
    }

    // ======= CRC =========

    public delegate IntPtr CheckFileStatePrototype(IntPtr unk1, ulong crc64);

    public Hook<CheckFileStatePrototype> CheckFileStateHook { get; private set; }

    private nint CheckFileStateDetour(nint ptr, ulong crc64)
    {
        if (CustomMdlCrc.Contains(crc64)) return CustomFileFlag;
        if (CustomTexCrc.Contains(crc64)) TexReturnData.Value = true;
        if (CustomScdCrc.Contains(crc64)) ScdReturnData.Value = true;
        return CheckFileStateHook.Original(ptr, crc64);
    }

    public void AddCrc(ResourceType type, FullPath? path)
    {
        _ = type switch
        {
            ResourceType.Mdl when path.HasValue => CustomMdlCrc.Add(path.Value.Crc64),
            ResourceType.Tex when path.HasValue => CustomTexCrc.Add(path.Value.Crc64),
            ResourceType.Scd when path.HasValue => CustomScdCrc.Add(path.Value.Crc64),
            _ => false,
        };
    }

    // ======= LOAD SCD =============

    private delegate byte SoundOnLoadDelegate(ResourceHandle* handle, SeFileDescriptor* descriptor, byte unk);

    [Signature(LoadScdLocalSig)]
    private readonly delegate* unmanaged<ResourceHandle*, SeFileDescriptor*, byte, byte> LoadScdFileLocal = null!;

    [Signature(SoundOnLoadSig, DetourName = nameof(OnScdLoadDetour))]
    private readonly Hook<SoundOnLoadDelegate> SoundOnLoadHook = null!;

    private byte OnScdLoadDetour(ResourceHandle* handle, SeFileDescriptor* descriptor, byte unk)
    {
        var ret = SoundOnLoadHook.Original(handle, descriptor, unk);
        if (!ScdReturnData.Value) return ret;

        // Function failed on a replaced scd, call local.
        ScdReturnData.Value = false;
        return LoadScdFileLocal(handle, descriptor, unk);
    }
}
