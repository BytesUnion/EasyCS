class SuperMethodCallStatement : Statement
{
    public string MethodName { get; }
    public List<Expression> Arguments { get; }

    public SuperMethodCallStatement(string methodName, List<Expression> arguments)
    {
        MethodName = methodName;
        Arguments = arguments;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (variables.TryGetValue("cur", out var currentObject) && currentObject is EasyScriptObject esObject)
        {
            if (esObject.ContainsKey("__parent__") && esObject["__parent__"] is EasyScriptClass parentClass)
            {
                if (parentClass.Init.TryGetValue(MethodName, out var parentMethodStatement))
                {
                    var evaluatedArguments = new List<object>();
                    foreach (var argument in Arguments)
                    {
                        evaluatedArguments.Add(argument.Evaluate(variables, functions));
                    }

                    var parentInitCall = new InitCallExpression(parentMethodStatement, evaluatedArguments, esObject);
                    parentInitCall.Evaluate(variables, functions);
                }
                else
                {
                    throw new Exception($"Method '{MethodName}' not found in parent class");
                }
            }
            else
            {
                throw new Exception("Super class not found");
            }
        }
        else
        {
            throw new Exception("Current object ('cur') not found");
        }
    }
}
