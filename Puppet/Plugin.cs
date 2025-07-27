using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Puppet.PuppetMaster;
using Puppet.Windows;

namespace Puppet;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "Puppet";
    private const string CommandName = "/puppet";
    private ExcelSheet<Emote>? Emotes;

    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    public ServerChat RealChat { get; init; }

    public Configuration Configuration { get; init; }
    public WindowSystem WindowSystem = new("Puppet");

    private ConfigWindow ConfigWindow { get; init; }

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);

        DalamudApi.Initialize(PluginInterface);

        ConfigWindow = new ConfigWindow(this);
        //MainWindow = new MainWindow(this);

        WindowSystem.AddWindow(ConfigWindow);
        //WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "个人版Puppeteer"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        RealChat = new ServerChat(DalamudApi.SigScanner);

        DalamudApi.Chat.ChatMessage += ChatOnChatMessage;

        Emotes = DalamudApi.GameData.GetExcelSheet<Emote>();

#if DEBUG
        ConfigWindow.IsOpen = true;
#endif
    }

    private void ChatOnChatMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool ishandled)
    {
        if (!Configuration.Enabled) return;
        var channel = ChatChannel.GetChatChannelFromXivChatType(type);

        if (channel == null) return;
        if (Configuration.Trigger.IsNullOrEmpty()) return;
        if (!Configuration.ChannelsPuppeteer.Contains(channel)) return;
        var playerPayload = sender.Payloads.FirstOrDefault(x => x.Type == PayloadType.Player);
        var from = playerPayload is null ? string.Empty : ((PlayerPayload)playerPayload).DisplayedName;
        if (string.IsNullOrEmpty(from))
            from = DalamudApi.ClientState.LocalPlayer?.Name + "\ue05d" + DalamudApi.ClientState.LocalPlayer?.HomeWorld.Value.Name.ExtractText();

        switch ((ConfigWindow.OpenTo)Configuration.Target)
        {
            case ConfigWindow.OpenTo.仅目标:
                if (from != ConfigWindow.TargetName) return;
                break;
            case ConfigWindow.OpenTo.白名单:
                if (!Configuration.WhiteList.Contains(from)) return;
                break;
            case ConfigWindow.OpenTo.所有人:
#if DEBUG
                if (DalamudApi.ClientState.LocalPlayer?.Name.TextValue == sender.TextValue) break;
#endif
                if (from.IsNullOrEmpty()) return;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var str = message.TextValue;
        if (!Regex.IsMatch(str, Configuration.Trigger)) return;
        str = Regex.Replace(str, Configuration.Trigger, "").Trim();

        DalamudApi.Log.Debug($"Received message: {message.TextValue} from {sender.TextValue}.");

        var matched = false;

        foreach (var alias in Configuration.Aliases)
        {
            if (!alias.Enabled) continue;
            var msg = str;
            if (alias.From.IsNullOrEmpty()) continue;
            if (!Regex.IsMatch(msg, alias.From)) continue;
            matched = true;
            msg = alias.Replace(msg, alias);

            //是表情
            var emote = msg.Split(" ")[0];
            if (Emotes!.Any(x => x.Name == emote)) msg += " motion";

            HandleMessage(msg);
        }

        //全都不中
        if (!matched)
        {
            var msg = str;
            var emote = msg.Split(" ")[0];
            if (Emotes!.Any(x => x.Name == emote)) msg += " motion";
            HandleMessage(msg);
        }
    }

    private void HandleMessage(string message)
    {
        if (message.IsNullOrEmpty()) return;
        message = message.Replace("[", "<").Replace("]", ">");
        try
        {
            DalamudApi.Log.Information($"Sending Command: /{message}");
            RealChat.SendMessage("/" + message);
        }
        catch (Exception e)
        {
            DalamudApi.Log.Error("SendMessage Error:" + e);
        }
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow?.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just display our main ui
        ConfigWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}
