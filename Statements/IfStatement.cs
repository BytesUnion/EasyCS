class IfStatement : Statement
{
    public Expression Condition { get; }
    public List<Statement> ThenBlock { get; }
    public List<Statement> ElseBlock { get; }
    public List<IfStatement> ElifStatements { get; }

    public IfStatement(Expression condition, List<Statement> thenBlock, List<Statement> elseBlock, List<IfStatement> elifStatements)
    {
        Condition = condition;
        ThenBlock = thenBlock;
        ElseBlock = elseBlock;
        ElifStatements = elifStatements;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (Convert.ToBoolean(Condition.Evaluate(variables, functions)))
        {
            foreach (var statement in ThenBlock)
            {
                statement.Execute(variables, functions);
            }
        }
        else
        {
            bool elifExecuted = false;
            foreach (var elif in ElifStatements)
            {
                if (Convert.ToBoolean(elif.Condition.Evaluate(variables, functions)))
                {
                    foreach (var statement in elif.ThenBlock)
                    {
                        statement.Execute(variables, functions);
                    }
                    elifExecuted = true;
                    break;
                }
            }

            if (!elifExecuted && ElseBlock != null)
            {
                foreach (var statement in ElseBlock)
                {
                    statement.Execute(variables, functions);
                }
            }
        }
    }
}
