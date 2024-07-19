class IfStatement : Statement
{
    private Expression condition;
    private List<Statement> thenBlock;
    private List<Statement> elseBlock;

    public IfStatement(Expression condition, List<Statement> thenBlock, List<Statement> elseBlock)
    {
        this.condition = condition;
        this.thenBlock = thenBlock;
        this.elseBlock = elseBlock;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        bool conditionResult = Convert.ToBoolean(condition.Evaluate(variables, functions));

        if (conditionResult)
        {
            foreach (var statement in thenBlock)
            {
                statement.Execute(variables, functions);
            }
        }
        else if (elseBlock != null)
        {
            foreach (var statement in elseBlock)
            {
                statement.Execute(variables, functions);
            }
        }
    }
}
