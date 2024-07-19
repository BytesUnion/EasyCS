class ObjectPropertyAccessExpression : Expression
{
    public Expression Target { get; }
    public Expression Key { get; }

    public ObjectPropertyAccessExpression(Expression target, Expression key)
    {
        Target = target;
        Key = key;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var obj = EvaluateTarget(variables, functions);
        var keyValue = Key.Evaluate(variables, functions).ToString();

        if (obj is EasyScriptObject easyScriptObject)
        {
            if (easyScriptObject.ContainsKey(keyValue))
            {
                return easyScriptObject[keyValue];
            }
            else
            {
                throw new Exception($"Oh no! The key '{keyValue}' couldn't be found in the object.");
            }
        }
        else
        {
            throw new Exception("Oops! Can't access properties on something that's not an object");
        }
    }

    public dynamic EvaluateTarget(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetObj = Target.Evaluate(variables, functions);

        if (targetObj is EasyScriptObject easyScriptObject)
        {
            return easyScriptObject;
        }
        else if (targetObj is EasyScriptList easyScriptList)
        {
            return easyScriptList;
        }
        else if (targetObj is string targetName && variables.ContainsKey(targetName))
        {
            if (variables[targetName] is EasyScriptObject)
            {
                return (EasyScriptObject)variables[targetName];
            }
            if (variables[targetName] is EasyScriptList)
            {
                return (EasyScriptList)variables[targetName];
            }
        }

        throw new Exception("Trying to access a property of a non-object value. Make sure it's an object before accessing properties!");
    }
}
