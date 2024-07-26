class NewExpression : Expression
{
    private List<string> classPath;
    private List<Expression> arguments;

    public NewExpression(List<string> classPath, List<Expression> arguments)
    {
        this.classPath = classPath;
        this.arguments = arguments;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        object currentObj = null;
        EasyScriptClass classObj = null;

        foreach (var part in classPath)
        {
            if (currentObj == null)
            {
                if (!variables.TryGetValue(part, out currentObj))
                {
                    throw new Exception($"Class '{part}' not found");
                }
            }
            else
            {
                if (currentObj is EasyScriptObject instance)
                {
                    if (instance.ContainsKey(part))
                    {
                        currentObj = instance[part];
                    }
                    else
                    {
                        throw new Exception($"Nested class '{part}' not found");
                    }
                }
                else
                {
                    throw new Exception($"Invalid class path: '{string.Join(".", classPath)}'");
                }
            }
        }

        if (!(currentObj is EasyScriptClass))
        {
            throw new Exception($"'{string.Join(".", classPath)}' is not a valid class");
        }

        classObj = (EasyScriptClass)currentObj;
        return classObj.Instantiate(arguments, variables, functions);
    }
}