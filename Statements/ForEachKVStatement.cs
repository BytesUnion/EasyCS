class ForEachKVStatement : Statement
{
    private string keyVariable;
    private string valueVariable;
    private Expression collectionExpression;
    private List<Statement> body;

    public ForEachKVStatement(string keyVariable, string valueVariable, Expression collectionExpression, List<Statement> body)
    {
        this.keyVariable = keyVariable;
        this.valueVariable = valueVariable;
        this.collectionExpression = collectionExpression;
        this.body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var collection = collectionExpression.Evaluate(variables, functions);

        if (collection is EasyScriptObject easyScriptObject)
        {
            try
            {
                for (int i = 0; i < easyScriptObject.Count; i++)
                {
                    string key = easyScriptObject.GetKeyAt(i);
                    object value = easyScriptObject[key];
                    variables[keyVariable] = key;
                    variables[valueVariable] = value;
                    foreach (var statement in body)
                    {
                        statement.Execute(variables, functions);
                    }
                }
            }
            catch (BreakException)
            {
            }
        }
        else if (collection is String str)
        {
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int key = i;
                    object value = str[key].ToString();
                    variables[keyVariable] = key;
                    variables[valueVariable] = value;
                    foreach (var statement in body)
                    {
                        statement.Execute(variables, functions);
                    }
                }
            }
            catch (BreakException)
            {
            }
        }
        else
        {
            throw new Exception("Expression is not a collection");
        }
    }
}
