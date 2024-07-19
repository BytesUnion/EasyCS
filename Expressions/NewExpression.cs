class NewExpression : Expression
{
    private string className;
    private List<Expression> arguments;

    public NewExpression(string className, List<Expression> arguments)
    {
        this.className = className;
        this.arguments = arguments;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (!variables.TryGetValue(className, out var classObj) || !(classObj is EasyScriptClass))
        {
            throw new Exception($"Class '{className}' not found");
        }

        return ((EasyScriptClass)classObj).Instantiate(arguments, variables, functions);
    }

}