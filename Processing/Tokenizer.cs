using System.Text;
using System.Text.RegularExpressions;

public static class Tokenizer
{
    public static List<string> Tokenize(string script)
    {
        string pattern = @"\s+|([=+\-*/()><]=|!=|&&|\|\||
|\*\*)|([A-Za-z_][A-Za-z0-9_]*)|(\""[^\""]*\"")|(\'[^']*\'|\d+(\.\d*)?|\.\d+)|(\[|\]|\{|\}|,|:|\S)";

        var lines = script.Split('\n');

        var filteredScript = new StringBuilder();

        foreach (var line in lines)
        {
            filteredScript.AppendLine(Regex.Replace(line, @"(?m)>>.*$", ""));
        }

        return Regex.Matches(filteredScript.ToString(), pattern)
            .Cast<Match>()
            .Select(m => m.Value)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
    }
}
