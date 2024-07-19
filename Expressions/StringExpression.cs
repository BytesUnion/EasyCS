class StringExpression : Expression
{
    private string value;

    public StringExpression(string value)
    {
        this.value = value;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        return value;
    }
}