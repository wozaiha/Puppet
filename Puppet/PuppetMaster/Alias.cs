using System;
using System.Text.RegularExpressions;
using Dalamud.Utility;

namespace Puppet.PuppetMaster;
public enum AliasType
{
    普通, Gla预设, Gla单件, Customize, Moodles效果, Moodles预设
}
public class Alias
{
    public bool Enabled = false;
    public string From = String.Empty;
    public string To = String.Empty;
    public AliasType Type = AliasType.普通;
    public bool EnableAdv = false;

    public Alias(string from = "", string to = "", AliasType type = AliasType.普通)
    {
        From = from;
        To = to;
        Type = type;
    }

    public string Replace(string text, Alias alias)
    {
        DalamudApi.Log.Information($"Replacing:{text} with {alias.From} to {alias.To}");
        var from = alias.From;
        var to = alias.To;
        switch (alias.Type)
        {
            case AliasType.普通:
                from = $@"{from}";
                to = $@"{to}";
                break;
            case AliasType.Gla预设:
                from = $@".*{from}(\S*|[^,，]*).*";
                to = to.IsNullOrEmpty() ? $@"glamour apply $1 | [me]; true" : $@"glamour apply {to} | [me]; true";
                break;
            case AliasType.Gla单件:
                from = $@".*{from}(\S*|[^,，]*).*";
                to = to.IsNullOrEmpty() ? $@"glamour applyitem $1 | [me]" : $@"glamour applyitem {to} | [me]";
                break;
            case AliasType.Customize:
                if (To.IsNullOrEmpty()) return "";
                from = $@".*{from}(\S*|[^,，]*).*";
                to = $"c+ profile {(EnableAdv ? "enable" : "disable")} [me],{To} ";
                break;
            case AliasType.Moodles效果:
                if (To.IsNullOrEmpty()) return "";
                from = $@".*{from}(\S*|[^,，]*).*";
                to = $"moodle {(EnableAdv ? "apply" : "remove")} self moodle \"{To}\"";
                break;
            case AliasType.Moodles预设:
                if (To.IsNullOrEmpty()) return "";
                from = $@".*{from}(\S*|[^,，]*).*";
                to = $"moodle {(EnableAdv ? "apply" : "remove")} self preset \"{To}\"";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        var result = Regex.Replace(text, from, to);
        return result;
    }


}
