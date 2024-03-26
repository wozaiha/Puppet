using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Puppet.PuppetMaster;

namespace Puppet
{
    [Serializable]
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<ChatChannel.ChatChannels?> ChannelsPuppeteer { get; set; } = [];

        public List<string> WhiteList { get; set; } = [];

        public string Trigger = string.Empty;

        public List<Alias> Aliases { get; set; } = [];

        public int Target { get; set; } = 0;

        // the below exist just to make saving less cumbersome
        [NonSerialized]
        private DalamudPluginInterface? PluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.PluginInterface = pluginInterface;
        }

        public void Save()
        {
            this.PluginInterface!.SavePluginConfig(this);
        }
    }
}
