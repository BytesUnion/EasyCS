class WhileStatement : Statement
{
    private Expression condition;
    private List<Statement> body;

    public WhileStatement(Expression condition, List<Statement> body)
    {
        this.condition = condition;
        this.body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        try
        {
            while (Convert.ToBoolean(condition.Evaluate(variables, functions)))
            {
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
}