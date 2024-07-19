using System.Collections;

class ForEachStatement : Statement
{
    private string variable;
    private Expression iterableExpression;
    private List<Statement> body;

    public ForEachStatement(string variable, Expression iterableExpression, List<Statement> body)
    {
        this.variable = variable;
        this.iterableExpression = iterableExpression;
        this.body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        object iterable = iterableExpression.Evaluate(variables, functions);

        if (iterable is IEnumerable<object> collection)
        {
            try
            {
                foreach (object item in collection)
                {
                    variables[variable] = item;
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
        else if (iterable is IEnumerable collectionNonGeneric)
        {
            try
            {
                foreach (object item in collectionNonGeneric)
                {
                    variables[variable] = item;
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
        else if (iterable is String str)
        {
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    int key = i;
                    object value = str[key].ToString();
                    variables[variable] = value;
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
            throw new Exception($"Expression is not iterable: {iterable}");
        }
    }
}
