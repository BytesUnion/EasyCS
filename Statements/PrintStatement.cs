class PrintStatement : Statement
{
    private Expression expr;

    public PrintStatement(Expression expr)
    {
        this.expr = expr;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        object result = expr.Evaluate(variables, functions);
        Console.WriteLine(result == null ? "null" : result.ToString());
    }
}