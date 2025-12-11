// Adapted from https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Interop/ResourceLoader.Vfx.cs
// ac4aab8
using System;
using Dalamud.Hooking;

namespace VoicedNaelQuotes.Interop;

public unsafe partial class ResourceLoader
{
    public delegate IntPtr ActorVfxCreateDelegate(string path, IntPtr a2, IntPtr a3, float a4, char a5, ushort a6, char a7);
    public ActorVfxCreateDelegate ActorVfxCreate;

    public delegate IntPtr ActorVfxRemoveDelegate(IntPtr vfx, char a2);
    public ActorVfxRemoveDelegate ActorVfxRemove;
    public Hook<ActorVfxRemoveDelegate> ActorVfxRemoveHook { get; private set; }

    private IntPtr ActorVfxRemoveDetour(IntPtr vfx, char a2)
    {
        this.vfxSpawn?.InteropRemoved(vfx);
        return ActorVfxRemoveHook.Original(vfx, a2);
    }
}
