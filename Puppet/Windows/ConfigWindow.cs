using System;
using System.Numerics;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.Marshalling;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using Puppet.PuppetMaster;


namespace Puppet.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    private IFontHandle GameFont;

    private PlayerCharacter? Target => (PlayerCharacter) DalamudApi.Targets.Target;
    
    public string? TargetName => $"{Target?.Name.TextValue}\ue05d{Target?.HomeWorld.GameData?.Name}";

    public ConfigWindow(Plugin plugin) : base(
        "Puppeteer 设置"
        )
    {
        //this.Size = new Vector2(232, 75);
        //this.SizeCondition = ImGuiCond.Always;

        this.Configuration = plugin.Configuration;
        GameFont = DalamudApi.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamilyAndSize.Axis18));
        
    }

    public void Dispose() { }

    private string[] AliasHeaders = { "启用", "原内容", "替换为", "类型","删除" };
    private void AliasTab()
    {
        if (ImGui.BeginTable("AliasList", 5,ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders ))
        {
            foreach (var header in AliasHeaders)
            {
                ImGui.TableSetupColumn(header);
            }
            ImGui.TableHeadersRow();

            var changed = false;
            var i = 0;
            foreach (var alias in Configuration.Aliases)
            {
                ImGui.TableNextColumn();
                changed |= ImGui.Checkbox($"##Enabled{i}", ref Configuration.Aliases[i].Enabled);
                ImGui.TableNextColumn();
                var width = ImGui.GetColumnWidth();
                ImGui.SetNextItemWidth(width);
                changed |= ImGui.InputText($"##From{i}",ref Configuration.Aliases[i].From, 100);
                ImGui.TableNextColumn();
                if (alias.Type == AliasType.GlamourerApply)
                {
                    ImGui.BeginDisabled();
                    ImGui.Text($"Glamourer切换预设");
                    ImGui.EndDisabled();
                }
                else
                {
                    width = ImGui.GetColumnWidth();
                    ImGui.SetNextItemWidth(width);
                    changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                }
                ImGui.TableNextColumn();
                width = ImGui.GetColumnWidth();
                ImGui.SetNextItemWidth(width);
                var selected = (int)Configuration.Aliases[i].Type;
                if (ImGui.Combo($"##Type{i}", ref selected, new string[2] {"普通", "Gla穿衣"}, 2))
                {
                    Configuration.Aliases[i].Type = (AliasType)selected;
                    changed = true;
                }

                ImGui.TableNextColumn();
                if (!ImGui.GetIO().KeyCtrl) ImGui.BeginDisabled();
                if (ImGui.Button($"删除##{i}"))
                {
                    changed = true;
                    Configuration.Aliases.RemoveAt(i);
                    break;
                }
                if (!ImGui.GetIO().KeyCtrl) ImGui.EndDisabled();
                i++;
            }
            ImGui.TableNextColumn();
            if (ImGui.Button($"添加##AddAlias"))
            {
                changed = true;
                Configuration.Aliases.Add(new Alias());
            }
            

            ImGui.EndTable();
            if (changed) Configuration.Save();

        }
    }

    public enum WhiteList
    {
        仅目标,白名单,白名单及好友,所有人
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
            ImGui.Separator();
        }

        ImGui.Text("权限设置:");
        var current = Configuration.Target;
        for (int i = 0; i < 4; i++)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton($"{(WhiteList)i}", ref current, i))
            {
                Configuration.Target = current;
                Configuration.Save();
            }
        }
        ImGui.Separator();
    }

    private void SettingTab()
    {
        ImGui.Text("Puppeteer Channels:");
        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip("Every selected channel from here becomes a channel that you will pick up your trigger word from.");
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
            // Move to the next row if it is 通讯贝1 or 跨服通讯贝S1
            if (f is ChatChannel.ChatChannels.通讯贝1 or ChatChannel.ChatChannels.跨服通讯贝1)
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

    }
}
