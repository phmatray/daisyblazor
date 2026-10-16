using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DaisyBlazor.Docs.Docs;

/// <summary>
/// A small tokenizer for Razor, C#, CSS, XML and shell snippets, emitting <c>tok-*</c> spans whose colours
/// derive from the active daisyUI theme. Good enough for documentation; not a full parser.
/// </summary>
public static partial class RazorHighlighter
{
    public static string Highlight(string code, string language = "razor") => language switch
    {
        "csharp" or "cs" => Tokens(code, CSharp()),
        "css" => Tokens(code, Css()),
        "bash" or "shell" or "sh" => Tokens(code, Shell()),
        "xml" or "html" => Tokens(code, Markup()),
        _ => RazorWithCode(code),
    };

    /// <summary>Markup gets the Razor tokenizer (no C# keywords, so prose stays prose); an @code block gets C#.</summary>
    private static string RazorWithCode(string code)
    {
        int at = code.IndexOf("@code", StringComparison.Ordinal);
        if (at < 0) return Tokens(code, Razor());
        return Tokens(code[..at], Razor())
            + "<span class=\"tok-razor\">@code</span>"
            + Tokens(code[(at + 5)..], CSharp());
    }

    private static string Tokens(string code, Regex rx)
    {
        var sb = new StringBuilder(code.Length * 2);
        int last = 0;
        foreach (Match m in rx.Matches(code))
        {
            if (m.Index > last) sb.Append(WebUtility.HtmlEncode(code[last..m.Index]));
            string? cls = null;
            foreach (string name in new[] { "com", "str", "razor", "tag", "attr", "kw", "num" })
            {
                if (m.Groups[name].Success) { cls = name; break; }
            }
            if (cls is null)
            {
                sb.Append(WebUtility.HtmlEncode(m.Value));
            }
            else
            {
                sb.Append("<span class=\"tok-").Append(cls).Append("\">")
                  .Append(WebUtility.HtmlEncode(m.Value)).Append("</span>");
            }
            last = m.Index + m.Length;
        }
        if (last < code.Length) sb.Append(WebUtility.HtmlEncode(code[last..]));
        return sb.ToString();
    }

    private const string CsKeywords =
        "abstract|async|await|bool|break|case|catch|class|const|continue|decimal|default|double|else|enum|false|finally|float|for|foreach|get|if|in|init|int|interface|internal|is|long|namespace|new|null|object|out|override|partial|private|protected|public|readonly|record|required|return|sealed|set|static|string|switch|this|throw|true|try|typeof|using|var|void|when|while|with|yield";

    [GeneratedRegex(
        """(?<com>@\*[\s\S]*?\*@|<!--[\s\S]*?-->|//[^\n]*)"""
        + """|(?<str>"(?:[^"\\\n]|\\.)*")"""
        + """|(?<razor>@(?:code|page|inject|using|inherits|implements|typeparam|attribute|rendermode|layout|namespace|foreach|if|else|for|while|switch|bind(?::\w+)?|on\w+(?::\w+)?|ref|key)\b|@[A-Za-z_][\w.]*(?:\([^)\n]*\))?|@\(|@\{)"""
        + """|(?<tag></?[A-Za-z][\w.:-]*|/?>)"""
        + """|(?<attr>\b[A-Za-z_][\w:-]*(?==))""")]
    private static partial Regex Razor();

    [GeneratedRegex(
        """(?<com>//[^\n]*|/\*[\s\S]*?\*/)"""
        + """|(?<str>@?"(?:[^"\\\n]|\\.)*"|'(?:[^'\\]|\\.)')"""
        + """|(?<razor>\[[A-Z]\w*(?:\([^\]\n]*\))?\])"""
        + """|(?<kw>\b(?:""" + CsKeywords + """)\b)"""
        + """|(?<num>\b\d+(?:\.\d+)?[mdfMDF]?\b)""")]
    private static partial Regex CSharp();

    [GeneratedRegex(
        """(?<com>/\*[\s\S]*?\*/)"""
        + """|(?<str>"[^"\n]*"|'[^'\n]*')"""
        + """|(?<razor>@[\w-]+)"""
        + """|(?<attr>--[\w-]+|[\w-]+(?=\s*:))"""
        + """|(?<num>\b\d+(?:\.\d+)?(?:px|rem|em|%|ms|s)?\b)""")]
    private static partial Regex Css();

    [GeneratedRegex(
        """(?<com>#[^\n]*)"""
        + """|(?<str>"[^"\n]*"|'[^'\n]*')"""
        + """|(?<kw>^\s*(?:dotnet|npm|npx|git|cd|mkdir)\b)"""
        + """|(?<attr>\s--?[\w-]+)""",
        RegexOptions.Multiline)]
    private static partial Regex Shell();

    [GeneratedRegex(
        """(?<com><!--[\s\S]*?-->)"""
        + """|(?<str>"[^"\n]*")"""
        + """|(?<tag></?[A-Za-z][\w.:-]*|/?>)"""
        + """|(?<attr>\b[A-Za-z_][\w:-]*(?==))""")]
    private static partial Regex Markup();
}
