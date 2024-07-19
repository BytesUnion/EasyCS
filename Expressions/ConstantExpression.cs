class ConstantExpression : Expression
{
    private object value;

    public ConstantExpression(object value)
    {
        this.value = value;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        return value;
    }
}