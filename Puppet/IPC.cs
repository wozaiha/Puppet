using Dalamud.Plugin.Ipc;
using System;

namespace Puppet;

public class PuppetIpc : IDisposable
{
    public const string LabelGetDesignList = "Glamourer.GetDesignList";

    public readonly ICallGateSubscriber<(string Name, Guid Identifier)[]> GetDesignList =
        DalamudApi.PluginInterface.GetIpcSubscriber<(string Name, Guid Identifier)[]>(LabelGetDesignList);

    public void Dispose() { }
}
