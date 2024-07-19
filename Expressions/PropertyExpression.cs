class PropertyExpression : Expression
{
    private string objectName;
    private string propertyName;

    public PropertyExpression(string objectName, string propertyName)
    {
        this.objectName = objectName;
        this.propertyName = propertyName;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (!variables.ContainsKey(objectName) || !(variables[objectName] is EasyScriptObject))
        {
            throw new Exception($"'{objectName}' isn't a valid object. Check its type.");
        }

        var obj = (EasyScriptObject)variables[objectName];
        return obj.get(propertyName);
    }
}
