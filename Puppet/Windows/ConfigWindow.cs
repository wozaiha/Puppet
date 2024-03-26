using System;
using System.Numerics;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Puppet.PuppetMaster;


namespace Puppet.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    private IFontHandle GameFont;

    public ConfigWindow(Plugin plugin) : base(
        "Puppeteer 设置",
        //ImGuiWindowFlags.AlwaysAutoResize | 
        ImGuiWindowFlags.NoCollapse | 
        ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse)
    {
        //this.Size = new Vector2(232, 75);
        //this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
        GameFont = DalamudApi.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamilyAndSize.Axis18));
    }

    public void Dispose() { }

    private void AliasTab()
    {
        
    }


    private void PuppeteerTab()
    {
        using (GameFont?.Push())
        {
            ImGui.Text("触发词:");
            ImGui.SameLine();
            if (ImGui.InputText("##trigger", ref Configuration.Trigger, 64))
            {
                Configuration.Save();
            }
        }
    }

    private void SettingTab()
    {
        ImGui.Text("Puppeteer Channels:");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Every selected channel from here becomes a channel that you will pick up your trigger word from.\n" +
                             "The Global Puppeteer trigger works in all channels, and cannot be configured.");
        }
        var j = 0;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 2 * ImGuiHelpers.GlobalScale);
        foreach (var f in ChatChannel.GetOrderedChannels())
        {
            // See if it is already enabled by default
            var enabledPuppet = Configuration.ChannelsPuppeteer.Contains(f);
            // Create a new line after every 4 columns
            if (j != 0 && (j == 4 || j == 7 || j == 11 || j == 15 || j == 19))
            {
                ImGui.NewLine();
                //i = 0;
            }
            // Move to the next row if it is LS1 or CWLS1
            if (f is ChatChannel.ChatChannels.LS1 or ChatChannel.ChatChannels.CWL1)
                ImGui.Separator();

            if (ImGui.Checkbox($"{f}##{f}_puppeteer", ref enabledPuppet))
            {
                // See If the UIHelpers.Checkbox is clicked, If not, add to the list of enabled channels, otherwise, remove it.
                if (enabledPuppet) Configuration.ChannelsPuppeteer.Add(f);
                else Configuration.ChannelsPuppeteer.Remove(f);
                Configuration.Save();
            }

            ImGui.SameLine();
            j++;
        }
    }

    public override void Draw()
    {
            if (ImGui.BeginTabBar("All Tabs"))
            {
                if (ImGui.BeginTabItem("Puppet"))
                {
                    PuppeteerTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Alias"))
                {
                    AliasTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Settings"))
                {
                    SettingTab();
                    ImGui.EndTabItem();
                }
                ImGui.EndTabBar();
            }
            ImGui.End();

    }
}
