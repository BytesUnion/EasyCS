class UnaryExpression : Expression
{
    private string op;
    private Expression expr;

    public UnaryExpression(string op, Expression expr)
    {
        this.op = op;
        this.expr = expr;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        dynamic value = expr.Evaluate(variables, functions);
        return op switch
        {
            "-" => -value,
            _ => throw new Exception($"'{op}' isn't a valid unary operator. Double-check your syntax.")
        };
    }
}
