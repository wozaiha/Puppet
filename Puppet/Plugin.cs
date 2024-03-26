using System;
using System.Linq;
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
using Lumina.Excel.GeneratedSheets;
using Puppet.PuppetMaster;
using Puppet.Windows;

namespace Puppet
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Puppet";
        private const string CommandName = "/puppet";
        private ExcelSheet<Emote>? Emotes;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public RealChatInteraction RealChat { get; init; }

        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("Puppet");

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            DalamudApi.Initialize(PluginInterface);

            ConfigWindow = new ConfigWindow(this);
            //MainWindow = new MainWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            //WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "个人版Puppeteer"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

            RealChat = new RealChatInteraction(DalamudApi.SigScanner);

            DalamudApi.Chat.ChatMessage += ChatOnChatMessage;

            Emotes = DalamudApi.GameData.GetExcelSheet<Emote>();

#if DEBUG
            ConfigWindow.IsOpen = true;
#endif

        }

        private void ChatOnChatMessage(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool ishandled)
        {
            var channel = ChatChannel.GetChatChannelFromXivChatType(type); 

            if (channel == null) return;
            if (Configuration.Trigger.IsNullOrEmpty()) return;
            if (!Configuration.ChannelsPuppeteer.Contains(channel)) return;

            var from = ((PlayerPayload)sender.Payloads.Where(x => x.Type == PayloadType.Player)?.FirstOrDefault())?.DisplayedName;

            switch ((ConfigWindow.OpenTo)Configuration.Target)
            {
                case ConfigWindow.OpenTo.仅目标:
                    if (from != ConfigWindow.TargetName) return;
                    break;
                case ConfigWindow.OpenTo.白名单:
                    if (!Configuration.WhiteList.Contains(from)) return;
                    break;
                case ConfigWindow.OpenTo.所有人:
                    if (from.IsNullOrEmpty() && DalamudApi.ClientState.LocalPlayer?.Name.TextValue != "旋羽翼") return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var msg = message.TextValue;
            if (!msg.Contains(Configuration.Trigger)) return;
            msg = msg.Replace(Configuration.Trigger, "").Trim();
            foreach (var alias in Configuration.Aliases)
            {
                msg = alias.Replace(msg, alias);
            }

            HandleMessage(msg, from);
            //ishandled = true;
        }

        private void HandleMessage(string message, string? sender)
        {
            message = message.Replace("[", "<").Replace("]", ">");
            //DalamudApi.Log.Warning($"Handle:from {sender} : {message} : from target = {sender == ConfigWindow.TargetName}");
            

            //是表情
            var emote = message.Split(" ")[0];
            if (Emotes!.Any(x => x.Name == emote))
            {
                message += " motion";
            }

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
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow?.Dispose();
            MainWindow?.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            ConfigWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
