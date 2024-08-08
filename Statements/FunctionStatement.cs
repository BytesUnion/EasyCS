class FunctionStatement : Statement
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }
    public Dictionary<string, object> OriginalContext { get; }
    public bool IsShared { get; set; }

    public FunctionStatement(string name, List<string> parameters, List<Statement> body, Dictionary<string, object> context)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
        OriginalContext = context != null ? new Dictionary<string, object>(context) : new Dictionary<string, object>();
        IsShared = false;
    }

    public static string DictionaryToString(Dictionary<string, object> dictionary)
    {
        List<string> entries = new List<string>();
        foreach (var kvp in dictionary)
        {
            entries.Add($"{kvp.Key}: {kvp.Value}");
        }
        return "{" + string.Join(", ", entries) + "}";
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        Console.WriteLine(DictionaryToString(OriginalContext));
        functions[Name] = this;
    }
}
