class FunctionCallStatement : Statement
{
    private string functionName;
    private List<Expression> arguments;

    public FunctionCallStatement(string functionName, List<Expression> arguments)
    {
        this.functionName = functionName;
        this.arguments = arguments;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (!functions.ContainsKey(functionName))
        {
            throw new Exception($"Function '{functionName}' not defined");
        }

        FunctionStatement function = functions[functionName];
        Dictionary<string, object> localVariables = new Dictionary<string, object>();

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            localVariables[function.Parameters[i]] = arguments[i].Evaluate(variables, functions);
        }

        foreach (var statement in function.Body)
        {
            statement.Execute(localVariables, functions);
            if (localVariables.ContainsKey("return"))
            {
                if (functionName == variables.GetValueOrDefault("current_function", ""))
                {
                    variables["return"] = localVariables["return"];
                }
                return;
            }
        }

        if (functionName == variables.GetValueOrDefault("current_function", ""))
        {
            variables.Remove("return");
        }
    }
}