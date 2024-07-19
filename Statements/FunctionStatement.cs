class FunctionStatement : Statement
{
    public string Name { get; }
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }

    public FunctionStatement(string name, List<string> parameters, List<Statement> body)
    {
        Name = name;
        Parameters = parameters;
        Body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        functions[Name] = this;
    }
}