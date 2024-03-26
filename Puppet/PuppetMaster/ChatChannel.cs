using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
// this is the agent that handles the chatlog
// this is the framework that the game uses to handle all of its UI
// this is used for the enum
// this is used for the lists

// this is used for the lists

namespace Puppet.PuppetMaster;

/// <summary> This class is used to handle the chat channels for the GagSpeak plugin. It makes use of chatlog agent pointers, and is fairly complex, so would recommend not using yourself until you know why it points to what it does. </summary>
public static class ChatChannel
{
    // this is the agent that handles the chatlog
    private static unsafe AgentChatLog* ChatlogAgent = (AgentChatLog*)Framework.Instance()->GetUiModule()->GetAgentModule()->GetAgentByInternalId(AgentId.ChatLog);

    // this is the enum that handles the chat channels
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    sealed class EnumOrderAttribute : Attribute {
        public int Order { get; }
        public EnumOrderAttribute(int order) {
            Order = order;
        }
    }

    /// <summary> This enum is used to handle the chat channels. </summary>
    public enum ChatChannels
    {
        [EnumOrder(0)]
        Tell_In = 0,

        [EnumOrder(1)]
        悄悄话 = 17,

        [EnumOrder(2)]
        说 = 1,

        [EnumOrder(3)]
        小队 = 2,

        [EnumOrder(4)]
        团队 = 3,

        [EnumOrder(5)]
        Yell = 4,

        [EnumOrder(6)]
        Shout = 5,

        [EnumOrder(7)]
        部队 = 6,

        [EnumOrder(8)]
        新人频道 = 8,

        [EnumOrder(9)]
        跨服通讯贝1 = 9,

        [EnumOrder(10)]
        跨服通讯贝2 = 10,

        [EnumOrder(11)]
        跨服通讯贝3 = 11,

        [EnumOrder(12)]
        跨服通讯贝4 = 12,

        [EnumOrder(13)]
        跨服通讯贝5 = 13,

        [EnumOrder(14)]
        跨服通讯贝6 = 14,

        [EnumOrder(15)]
        跨服通讯贝7 = 15,

        [EnumOrder(16)]
        跨服通讯贝8 = 16,

        [EnumOrder(17)]
        通讯贝1 = 19,

        [EnumOrder(18)]
        通讯贝2 = 20,

        [EnumOrder(19)]
        通讯贝3 = 21,

        [EnumOrder(20)]
        通讯贝4 = 22,

        [EnumOrder(21)]
        通讯贝5 = 23,

        [EnumOrder(22)]
        通讯贝6 = 24,

        [EnumOrder(23)]
        通讯贝7 = 25,

        [EnumOrder(24)]
        通讯贝8 = 26,
    }

    /// <summary> This method is used to get the current chat channel. </summary>
    public static ChatChannels GetChatChannel() {
        // this is the channel that we are going to return
        ChatChannels channel;
        // this is unsafe code, so we need to use unsafe
        unsafe {
            channel = (ChatChannels)ChatlogAgent->CurrentChannel;
        }
        //return the channel now using
        return channel;
    }

    /// <summary> This method is used to get the ordered list of channels. </summary>
    public static IEnumerable<ChatChannels> GetOrderedChannels() {
        return Enum.GetValues(typeof(ChatChannels))
                .Cast<ChatChannels>()
                .Where(e => e != ChatChannels.Tell_In && e != ChatChannels.新人频道)
                .OrderBy(e => GetOrder(e));
    }

    // Match Channel types with command aliases for them
    public static string[] GetChannelAlias(this ChatChannels channel) => channel switch
    {
        ChatChannels.悄悄话 => new[] { "/t", "/tell"},
        ChatChannels.说 => new[] { "/s", "/say" },
        ChatChannels.小队 => new[] { "/p", "/party" },
        ChatChannels.团队 => new[] { "/a", "/alliance" },
        ChatChannels.Yell => new[] { "/y", "/yell" },
        ChatChannels.Shout => new[] { "/sh", "/shout" },
        ChatChannels.部队 => new[] { "/fc", "/freecompany" },
        ChatChannels.新人频道 => new[] { "/n", "/novice" },
        ChatChannels.跨服通讯贝1 => new[] { "/cwl1", "/cwlinkshell1" },
        ChatChannels.跨服通讯贝2 => new[] { "/cwl2", "/cwlinkshell2" },
        ChatChannels.跨服通讯贝3 => new[] { "/cwl3", "/cwlinkshell3" },
        ChatChannels.跨服通讯贝4 => new[] { "/cwl4", "/cwlinkshell4" },
        ChatChannels.跨服通讯贝5 => new[] { "/cwl5", "/cwlinkshell5" },
        ChatChannels.跨服通讯贝6 => new[] { "/cwl6", "/cwlinkshell6" },
        ChatChannels.跨服通讯贝7 => new[] { "/cwl7", "/cwlinkshell7" },
        ChatChannels.跨服通讯贝8 => new[] { "/cwl8", "/cwlinkshell8" },
        ChatChannels.通讯贝1 => new[] { "/l1", "/linkshell1" },
        ChatChannels.通讯贝2 => new[] { "/l2", "/linkshell2" },
        ChatChannels.通讯贝3 => new[] { "/l3", "/linkshell3" },
        ChatChannels.通讯贝4 => new[] { "/l4", "/linkshell4" },
        ChatChannels.通讯贝5 => new[] { "/l5", "/linkshell5" },
        ChatChannels.通讯贝6 => new[] { "/l6", "/linkshell6" },
        ChatChannels.通讯贝7 => new[] { "/l7", "/linkshell7" },
        ChatChannels.通讯贝8 => new[] { "/l8", "/linkshell8" },
        _ => Array.Empty<string>(),
    };

    // Get a commands list for given channelList(config) and add extra space for matching to avoid matching emotes.
    public static List<string> GetChatChannelsListAliases(this IEnumerable<ChatChannels> chatChannelsList)
    {
        var result = new List<string>();
        foreach (ChatChannels chatChannel in chatChannelsList)
        {
            result.AddRange(chatChannel.GetChannelAlias().Select(str => str + " "));
        }
        return result;
    }

    // see if the passed in alias is present as an alias in any of our existing channels
    public static bool IsAliasForAnyActiveChannel(this IEnumerable<ChatChannels> enabledChannels, string alias)
    {
        return enabledChannels.Any(channel => channel.GetChannelAlias().Contains(alias));
    }

    // get the chat channel type from the XIVChatType
    public static ChatChannels? GetChatChannelFromXivChatType(XivChatType type) {
        return type switch
        {
            XivChatType.TellIncoming    => ChatChannels.悄悄话,
            XivChatType.TellOutgoing    => ChatChannels.悄悄话,
            XivChatType.Say             => ChatChannels.说,
            XivChatType.Party           => ChatChannels.小队,
            XivChatType.Alliance        => ChatChannels.团队,
            XivChatType.Yell            => ChatChannels.Yell,
            XivChatType.Shout           => ChatChannels.Shout,
            XivChatType.FreeCompany     => ChatChannels.部队,
            XivChatType.NoviceNetwork   => ChatChannels.新人频道,
            XivChatType.Ls1             => ChatChannels.通讯贝1,
            XivChatType.Ls2             => ChatChannels.通讯贝2,
            XivChatType.Ls3             => ChatChannels.通讯贝3,
            XivChatType.Ls4             => ChatChannels.通讯贝4,
            XivChatType.Ls5             => ChatChannels.通讯贝5,
            XivChatType.Ls6             => ChatChannels.通讯贝6,
            XivChatType.Ls7             => ChatChannels.通讯贝7,
            XivChatType.Ls8             => ChatChannels.通讯贝8,
            XivChatType.CrossLinkShell1 => ChatChannels.跨服通讯贝1,
            XivChatType.CrossLinkShell2 => ChatChannels.跨服通讯贝2,
            XivChatType.CrossLinkShell3 => ChatChannels.跨服通讯贝3,
            XivChatType.CrossLinkShell4 => ChatChannels.跨服通讯贝4,
            XivChatType.CrossLinkShell5 => ChatChannels.跨服通讯贝5,
            XivChatType.CrossLinkShell6 => ChatChannels.跨服通讯贝6,
            XivChatType.CrossLinkShell7 => ChatChannels.跨服通讯贝7,
            XivChatType.CrossLinkShell8 => ChatChannels.跨服通讯贝8,
            _ => null
        };
    }

    /// <summary> This method is used to get the order of the enum, which is then given to getOrderedChannels. </summary>
    private static int GetOrder(ChatChannels channel) {
        // get the attribute of the channel
        var attribute = channel.GetType()
            .GetField(channel.ToString())
            ?.GetCustomAttributes(typeof(EnumOrderAttribute), false)
            .FirstOrDefault() as EnumOrderAttribute;
        // return the order of the channel, or if it doesnt have one, return the max value
        return attribute?.Order ?? int.MaxValue;
    }
}
