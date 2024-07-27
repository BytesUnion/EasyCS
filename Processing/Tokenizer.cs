using System.Text;
using System.Text.RegularExpressions;

public static class Tokenizer
{
    public static List<string> Tokenize(string script)
    {
        string pattern = @"(>>.*)|([=+\-*/()><]=|!=|&&|\|\||//|\*\*)|([A-Za-z_][A-Za-z0-9_]*)|(\""[^\""]*\"")|(\'[^']*\'|\d+(\.\d*)?|\.\d+)|(\[|\]|\{|\}|,|:)|\S";

        var matches = Regex.Matches(script, pattern, RegexOptions.Multiline);

        return matches
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}
