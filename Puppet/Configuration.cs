using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Puppet.PuppetMaster;

namespace Puppet;

[Serializable]
public class Configuration : IPluginConfiguration
{
    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private IDalamudPluginInterface? PluginInterface;

    public bool Enabled = false;

    public string Trigger = string.Empty;

    public List<ChatChannel.ChatChannels?> ChannelsPuppeteer { get; set; } = [];

    public List<string?> WhiteList { get; set; } = [];

    public List<Alias> Aliases { get; set; } = [];

    public int Target { get; set; } = 0;
    public int Version { get; set; } = 0;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
    }

    public void Save()
    {
        PluginInterface!.SavePluginConfig(this);
        
    }
}
