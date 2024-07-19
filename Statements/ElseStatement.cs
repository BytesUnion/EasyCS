class ElseStatement : Statement
{
    public List<Statement> Body { get; }

    public ElseStatement(List<Statement> body)
    {
        Body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        foreach (var statement in Body)
        {
            statement.Execute(variables, functions);
        }
    }
}