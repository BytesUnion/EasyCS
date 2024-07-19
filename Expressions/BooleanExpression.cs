class BooleanExpression : Expression
{
    private bool value;

    public BooleanExpression(bool value)
    {
        this.value = value;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        return value;
    }
}