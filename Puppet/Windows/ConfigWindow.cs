using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ImGuiNET;
using Puppet.PuppetMaster;


namespace Puppet.Windows;

public class ConfigWindow : Window, IDisposable
{
    private Configuration Configuration;

    private IFontHandle GameFont;

    private PuppetIpc Ipc;
    private List<string> glaPre;

    private IGameObject? Target => DalamudApi.Targets.Target;

    public string? TargetName => Target is IPlayerCharacter character
                                     ? $"{character?.Name.TextValue}\ue05d{character?.HomeWorld.GameData?.Name}"
                                     : "";

    public ConfigWindow(Plugin plugin) : base(
        "Puppet 设置"
    )
    {
        //this.Size = new Vector2(232, 75);
        //this.SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
        GameFont = DalamudApi.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(
            new GameFontStyle(GameFontFamilyAndSize.Axis18));
        Ipc = new PuppetIpc();
        glaPre = [];
    }

    public void Dispose() { }

    private string[] AliasHeaders = {"启用", "原内容", "替换为", "类型", "操作"};

    private void AliasTab()
    {
        if (ImGui.BeginTable("AliasList", AliasHeaders.Length, ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders))
        {
            foreach (var header in AliasHeaders) ImGui.TableSetupColumn(header);
            ImGui.TableHeadersRow();

            var changed = false;
            var i = 0;
            var needSort = -1;
            foreach (var alias in Configuration.Aliases)
            {
                ImGui.TableNextColumn();
                changed |= ImGui.Checkbox($"##Enabled{i}", ref Configuration.Aliases[i].Enabled);

                ImGui.TableNextColumn();
                var width = ImGui.GetColumnWidth();
                ImGui.SetNextItemWidth(width);
                changed |= ImGui.InputText($"##From{i}", ref Configuration.Aliases[i].From, 100);

                ImGui.TableNextColumn();
                switch (alias.Type)
                {
                    case AliasType.普通:
                        width = ImGui.GetColumnWidth();
                        ImGui.SetNextItemWidth(width);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"支持正则");
                        break;
                    case AliasType.Gla预设:
                        width = ImGui.GetColumnWidth();
                        ImGui.SetNextItemWidth(width);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"请填入Gla预设名,留空则对方可自主填写预设名为你替换");
                        break;
                    case AliasType.Gla单件:
                        width = ImGui.GetColumnWidth();
                        ImGui.SetNextItemWidth(width);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"请填入要更换的装备名,留空则对方可自主填写装备名为你替换");
                        break;
                    case AliasType.Customize:
                        width = ImGui.GetColumnWidth();
                        changed |= ImGui.Checkbox($"##EnableAdv{i}", ref Configuration.Aliases[i].EnableAdv);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选启用预设,不勾选禁用预设,预设名留空该项不生效");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width - 30f);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选启用预设,不勾选禁用预设,预设名留空该项不生效");
                        break;
                    case AliasType.Moodles效果:
                        width = ImGui.GetColumnWidth();
                        changed |= ImGui.Checkbox($"##EnableAdv{i}", ref Configuration.Aliases[i].EnableAdv);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选添加效果,不勾选移除效果,效果名留空该项不生效");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width - 30f);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选启用效果,不勾选禁用效果,效果名留空该项不生效\n须填写GUID或完整效果路径");
                        break;
                    case AliasType.Moodles预设:
                        width = ImGui.GetColumnWidth();
                        changed |= ImGui.Checkbox($"##EnableAdv{i}", ref Configuration.Aliases[i].EnableAdv);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选添加预设,不勾选移除预设,预设名留空该项不生效");
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(width - 30f);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"勾选启用预设,不勾选禁用预设,预设名留空该项不生效\n须填写GUID或完整预设路径");
                        break;
                    case AliasType.DB静态预设:
                        width = ImGui.GetColumnWidth();
                        ImGui.SetNextItemWidth(width);
                        changed |= ImGui.InputText($"##To{i}", ref Configuration.Aliases[i].To, 100);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip($"请填入DynamicBridge静态预设名,留空则恢复动态规则");
                        break;
                }

                

                ImGui.TableNextColumn();
                width = ImGui.GetColumnWidth();
                ImGui.SetNextItemWidth(width);
                var selected = (int)Configuration.Aliases[i].Type;
                if (ImGui.Combo($"##Type{i}", ref selected, Enum.GetNames(typeof(AliasType)),
                                Enum.GetNames(typeof(AliasType)).Length))
                {
                    Configuration.Aliases[i].Type = (AliasType)selected;
                    changed = true;
                }


                ImGui.TableNextColumn();
                DrawSortButton(i,ref needSort);
                
                if (!ImGui.GetIO().KeyCtrl) ImGui.BeginDisabled();
                if (ImGui.Button($"删除##{i}"))
                {
                    changed = true;
                    Configuration.Aliases.RemoveAt(i);
                    break;
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) ImGui.SetTooltip($"按住Ctrl+点击删除");
                if (!ImGui.GetIO().KeyCtrl) ImGui.EndDisabled();
                i++;
            }

            if (needSort > -1) Sort(needSort);

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

    private void DrawSortButton(int index, ref int needSort)
    {
        
        if (index == 0) ImGui.BeginDisabled();
        if (ImGui.ArrowButton($"##up{index}",ImGuiDir.Up))
        {
            needSort = index - 1;
        }

        if (index == 0) ImGui.EndDisabled();

        ImGui.SameLine();
        if (index == Configuration.Aliases.Count - 1) ImGui.BeginDisabled();
        if (ImGui.ArrowButton($"## down{index}", ImGuiDir.Down))
        {
            needSort = index;
        }

        if (index == Configuration.Aliases.Count - 1) ImGui.EndDisabled();

        ImGui.SameLine();
        
    }

    private void Sort(int index)
    {
        
        var alias = Configuration.Aliases[index];
        Configuration.Aliases.RemoveAt(index);
        Configuration.Aliases.Insert(index + 1, alias);
        Configuration.Save();
    }


    public enum OpenTo
    {
        仅目标,
        白名单,
        所有人
    }

    private void PuppeteerTab()
    {
        using (GameFont?.Push())
        {
            ImGui.Text("触发词:");
            ImGui.SameLine();
            if (ImGui.InputText("##trigger", ref Configuration.Trigger, 64)) Configuration.Save();
            ImGui.Separator();
        }

        ImGui.Text("权限设置:");
        var current = Configuration.Target;
        for (var i = 0; i < 3; i++)
        {
            ImGui.SameLine();
            if (ImGui.RadioButton($"{(OpenTo)i}", ref current, i))
            {
                Configuration.Target = current;
                Configuration.Save();
            }
        }

        ImGui.Separator();
    }

    private void SettingTab()
    {
        ImGui.Text("Puppeteer频道:");
        if (ImGui.IsItemHovered())
            ImGui.SetTooltip(
                "Every selected channel from here becomes a channel that you will pick up your trigger word from.");
        var j = 0;
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() - (2 * ImGuiHelpers.GlobalScale));
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

    private void WhiteListTab()
    {
        var y = ImGui.GetCursorPosY();
        ImGui.SetCursorPosY(y + 8f);
        ImGui.Text("当前目标:");
        ImGui.SameLine();
        ImGui.SetCursorPosY(y);
        var target = TargetName!;
        ImGui.SetNextItemWidth(300f);
        using (GameFont.Push())
        {
            ImGui.InputText($"##{target}", ref target, (uint)target.Length, ImGuiInputTextFlags.ReadOnly);
        }

        ImGui.SameLine();
        if (ImGui.Button("添加") && target.Length > 1)
        {
            Configuration.WhiteList.Add(target);
            Configuration.Save();
        }

        ImGui.Separator();

        if (ImGui.BeginTable($"WhiteListPlayer", 2, ImGuiTableFlags.Resizable | ImGuiTableFlags.Borders))
        {
            foreach (var s in new string[] {"角色名", "操作"}) ImGui.TableSetupColumn(s);
            ImGui.TableHeadersRow();

            var i = 0;
            foreach (var player in Configuration.WhiteList)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{player}");
                ImGui.TableNextColumn();
                if (ImGui.Button($"删除##{i}"))
                {
                    Configuration.WhiteList.RemoveAt(i);
                    break;
                }

                i++;
            }

            ImGui.EndTable();
        }

        ImGui.Separator();

        if (ImGui.BeginTable($"GlamourPresets", 2, ImGuiTableFlags.Borders))
        {
            ImGui.TableSetupColumn($"Gla预设");
            ImGui.TableSetupColumn("操作");
            ImGui.TableHeadersRow();
            var i = 0;
            foreach (var name in glaPre)
            {
                ImGui.TableNextColumn();
                ImGui.Text($"{name}");
                ImGui.TableNextColumn();
                if (ImGui.Button($"删除##预设{i}"))
                {
                    glaPre.RemoveAt(i);
                    break;
                }

                i++;
            }

            ImGui.EndTable();
        }

        if (ImGui.Button($"从Gla拉取数据"))
        {
            glaPre = Ipc.GetDesignList.InvokeFunc().Select(x => x.Value).ToList();
            ;
        }
        
        ImGui.SameLine();

        if (ImGui.Button($"导出到剪切板")) ImGui.SetClipboardText(string.Join(",", glaPre));

        ImGui.SameLine();
        if (ImGui.Button("从剪切板导入预设列表"))
            glaPre = ImGui.GetClipboardText().Split(",").Where(x => !x.IsNullOrEmpty()).ToList();
    }


    public override void Draw()
    {
        if (ImGui.Checkbox($"启用插件", ref Configuration.Enabled))
        {
            Configuration.Save();
            
        }

        if (!Configuration.Enabled) ImGui.BeginDisabled();
        if (ImGui.BeginTabBar("All Tabs"))
        {
            if (ImGui.BeginTabItem("Puppeteer"))
            {
                PuppeteerTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("转义"))
            {
                AliasTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("白名单"))
            {
                WhiteListTab();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("设置"))
            {
                SettingTab();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
        if (!Configuration.Enabled) ImGui.EndDisabled();
    }
}
