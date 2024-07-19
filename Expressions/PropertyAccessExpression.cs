class PropertyAccessExpression : Expression
{
    public Expression Target { get; }
    public string PropertyName { get; }

    public PropertyAccessExpression(Expression target, string propertyName)
    {
        Target = target;
        PropertyName = propertyName;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetValue = Target.Evaluate(variables, functions);

        if (targetValue is EasyScriptObject obj)
        {
            return obj[PropertyName];
        }
        throw new Exception("Invalid target type for property access. Only objects can be accessed with dot notation.");
    }
}