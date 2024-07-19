class ForStatement : Statement
{
    private string variableName;
    private Expression startValue;
    private Expression endValue;
    private List<Statement> body;

    public ForStatement(string variableName, Expression startValue, Expression endValue, List<Statement> body)
    {
        this.variableName = variableName;
        this.startValue = startValue;
        this.endValue = endValue;
        this.body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        int start = Convert.ToInt32(startValue.Evaluate(variables, functions));
        int end = Convert.ToInt32(endValue.Evaluate(variables, functions));

        for (int i = start; i <= end; i++)
        {
            variables[variableName] = i;
            try
            {
                foreach (var statement in body)
                {
                    statement.Execute(variables, functions);
                }
            }
            catch (BreakException)
            {
                break;
            }
        }
    }
}