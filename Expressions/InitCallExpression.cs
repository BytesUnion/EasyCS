class InitCallExpression : Expression
{
    public InitStatement initStat { get; set; }
    private List<object> arguments;
    private EasyScriptObject esObject;
    // private EasyScriptClass esClass;

    public InitCallExpression(InitStatement initst, List<object> arguments, EasyScriptObject esObject)
    {
        this.initStat = initst;
        this.arguments = arguments;
        this.esObject = esObject;
        // this.esClass = esClass;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        List<string> Parameters = initStat.Parameters;
        List<Statement> Body = initStat.Body;
        Dictionary<string, object> localVariables = new Dictionary<string, object>();

        localVariables["cur"] = this.esObject;
        localVariables["super"] = this.esObject.ContainsKey("super") ? this.esObject["super"] : null;

        if (localVariables["super"] is EasyScriptClass parentClass)
        {
            var parentInitStatement = parentClass.Init.GetValueOrDefault("__init__");
            if (parentInitStatement != null)
            {
                var parentArguments = new List<object>();
                var parentInitCall = new InitCallExpression(parentInitStatement, parentArguments, this.esObject);
                parentInitCall.Evaluate(localVariables, functions);
            }
        }

        for (int i = 0; i < Parameters.Count; i++)
        {
            localVariables[Parameters[i]] = arguments[i];
        }
        foreach (var statement in Body)
        {
            statement.Execute(localVariables, functions);
        }
        return null;
    }
}