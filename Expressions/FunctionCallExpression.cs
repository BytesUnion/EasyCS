class FunctionCallExpression : Expression
{
    private string functionName;
    private List<Expression> arguments;
    public FunctionCallExpression(string functionName, List<Expression> arguments)
    {
        this.functionName = functionName;
        this.arguments = arguments;
    }
    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (!functions.ContainsKey(functionName))
        {
            throw new Exception($"Function '{functionName}' is not defined");
        }
        FunctionStatement function = functions[functionName];
        Dictionary<string, object> localVariables = new Dictionary<string, object>(variables);
        for (int i = 0; i < function.Parameters.Count; i++)
        {
            localVariables[function.Parameters[i]] = arguments[i].Evaluate(variables, functions);
        }
        foreach (var statement in function.Body)
        {;
            statement.Execute(localVariables, functions);
            if (localVariables.ContainsKey("return"))
            {
                return localVariables["return"];
            }
        }
        return null;
    }
}