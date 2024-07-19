class VariableExpression : Expression
{
    private string variableName;
    public string Name => variableName;

    public VariableExpression(string variableName)
    {
        this.variableName = variableName;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (!variables.ContainsKey(variableName))
        {
            throw new Exception($"Variable '{variableName}' is not defined.");
        }
        var result = variables[variableName];
        return result;
    }
}