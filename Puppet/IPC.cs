using Dalamud.Plugin.Ipc;
using System;
using System.Collections.Generic;

namespace Puppet;

public class PuppetIpc : IDisposable
{
    public const string LabelGetDesignList = "Glamourer.GetDesignList.V2";

    public readonly ICallGateSubscriber<Dictionary<Guid, string>> GetDesignList =
        DalamudApi.PluginInterface.GetIpcSubscriber<Dictionary<Guid, string>>(LabelGetDesignList);

    public void Dispose() { }
}
