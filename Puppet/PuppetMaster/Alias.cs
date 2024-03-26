using System;
using System.Text.RegularExpressions;

namespace Puppet.PuppetMaster;
public enum AliasType
{
    Normal, GlamourerApply
}
public class Alias
{
    public bool Enabled = false;
    public string From = String.Empty;
    public string To = String.Empty;
    public AliasType Type = AliasType.Normal;

    public Alias(string from = "", string to = "", AliasType type = AliasType.Normal)
    {
        switch (type)
        {
            case AliasType.Normal:
                From = from;
                To = to;
                break;
            case AliasType.GlamourerApply:
                From = from;
                break;
            default:
                break;
        }
        Type = type;
    }

    public string Replace(string text, Alias alias)
    {
        var from = alias.From;
        var to = alias.To;
        switch (alias.Type)
        {
            case AliasType.Normal:
                from = $@"{from}";
                to = $@"{to}";
                break;
            case AliasType.GlamourerApply:
                from = $@"{from}(.*)";
                to = $@"glamour apply $1 | [me]; true";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }


        var result = Regex.Replace(text, from, to);
        return result;
    }


}
