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
        OriginalContext = new Dictionary<string, object>(context);
        IsShared = false;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        functions[Name] = this;
    }
}