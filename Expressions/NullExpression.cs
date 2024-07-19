class NullExpression : Expression
{
    public NullExpression() 
    {}
    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        return null;
    }
}