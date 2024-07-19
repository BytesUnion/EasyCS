class ObjectExpression : Expression
{
    public Dictionary<string, Expression> Properties { get; }

    public ObjectExpression(Dictionary<string, Expression> properties)
    {
        Properties = properties;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var obj = new EasyScriptObject();
        foreach (var kvp in Properties)
        {
            obj[kvp.Key] = kvp.Value.Evaluate(variables, functions);
        }
        return obj;
    }
}