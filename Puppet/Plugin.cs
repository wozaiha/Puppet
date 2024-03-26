using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Dalamud.Game.Command;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Logging;
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

        }

        private void ChatOnChatMessage(XivChatType type, uint senderid, ref SeString sender, ref SeString message, ref bool ishandled)
        {
            var channel = ChatChannel.GetChatChannelFromXivChatType(type); 

            if (channel == null) return;
            if (Configuration.Trigger.IsNullOrEmpty()) return;
            if (!Configuration.ChannelsPuppeteer.Contains(channel)) return;
            //if (!Configuration.WhiteList.Contains(sender.TextValue)) return;

            HandleMessage(message.TextValue, sender.TextValue);

            //ishandled = true;
        }

        private void HandleMessage(string message, string sender)
        {
            DalamudApi.Log.Warning($"Handle:from {sender} : {message}");

            var str = $@"(?<={Regex.Escape(Configuration.Trigger)} ).*";
            var regex = new Regex(str);
            var match = regex.Match(message);
            if (!match.Success) return;
            var command = match.Value;
            command = command.Replace("[", "<").Replace("]", ">");

            //是表情
            var emote = command.Split(" ")[0];
            if (Emotes!.Any(x => x.Name == emote))
            {
                command += " motion";
            }

            try
            {

#if DEBUG
                DalamudApi.Log.Warning($"Sending Command: /{command}");
#endif

                RealChat.SendMessage("/" + command);
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
