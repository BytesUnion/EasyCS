class BreakStatement : Statement
{
    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        throw new BreakException();
    }
}
