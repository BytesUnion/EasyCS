class InitStatement : Statement
{
    public Dictionary<string, InitStatement> inits;
    public List<string> Parameters { get; }
    public List<Statement> Body { get; }

    public InitStatement(List<string> parameters, List<Statement> body)
    {
        Parameters = parameters;
        Body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
    }

    public void addClass(string className)
    {
        inits[className] = this;
    }
}