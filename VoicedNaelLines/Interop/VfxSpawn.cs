// Adapted from https://github.com/0ceal0t/Dalamud-VFXEditor/blob/main/VFXEditor/Spawn/VfxSpawn.cs
// 70661e3
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using VoicedNaelLines.Interop.Structs;

namespace VoicedNaelLines.Interop;

public unsafe class VfxSpawn : IDisposable
{
    private record struct QueueItem(string path, IGameObject gameObject);

    private ResourceLoader resourceLoader;
    private readonly List<ActorVfx> spawnedVfxes = [];
    private readonly List<QueueItem> vfxSpawnQueue = [];
    private bool despawnVfxes = false;

    public VfxSpawn(ResourceLoader resourceLoader)
    {
        this.resourceLoader = resourceLoader;
        resourceLoader.vfxSpawn = this;

        Plugin.Framework.Update += OnFrameworkUpdate;
    }

    public void Dispose()
    {
        DespawnAllVfxes();
        OnFrameworkUpdate(Plugin.Framework);
        Plugin.Framework.Update -= OnFrameworkUpdate;
        resourceLoader.vfxSpawn = null;
    }

    private void OnFrameworkUpdate(IFramework framework)
    {

        foreach (var queueItem in vfxSpawnQueue)
        {
            if (queueItem.gameObject.IsValid())
            {
                var vfx = new ActorVfx(resourceLoader, queueItem.path);
                vfx.Create(queueItem.gameObject.Address, queueItem.gameObject.Address);
                spawnedVfxes.Add(vfx);
            }
        }
        vfxSpawnQueue.Clear();
        if (despawnVfxes)
        {
            DespawnAllVfxesFramework();
        }
    }

    public void DespawnAllVfxes() {
        despawnVfxes = true;
    }

    private void DespawnAllVfxesFramework()
    {
        foreach(var vfx in spawnedVfxes)
        {
            vfx.Remove();
        }
        spawnedVfxes.Clear();
        despawnVfxes = false;
    }

    public void QueueActorVfx(string path, IGameObject target)
    {
        vfxSpawnQueue.Add(new QueueItem(path, target));
    }

    public void InteropRemoved(IntPtr data)
    {
        if (!GetVfx(data, out var vfx)) { return; }

        spawnedVfxes.Remove((ActorVfx)vfx);

        if (vfx is ActorVfx actorVfx)
        {
            actorVfx.Vfx = null;
        }
    }

    public bool GetVfx(nint data, out BaseVfx vfx)
    {
        vfx = null!;
        if (data == IntPtr.Zero || spawnedVfxes.Count == 0) { return false; }

        vfx = spawnedVfxes.Find((vfx) => vfx.GetVfxPointer() == data)!;
        return vfx != null;

    }
}
