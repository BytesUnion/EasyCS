class ListAssignmentStatement : Statement
{
    public Expression Target { get; }
    public Expression Value { get; }

    public ListAssignmentStatement(Expression target, Expression value)
    {
        Target = target;
        Value = value;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var value = Value.Evaluate(variables, functions);

        if (Target is IndexAccessExpression indexExpr)
        {
            var list = indexExpr.Target.Evaluate(variables, functions);
            var index = Convert.ToInt32(indexExpr.Key.Evaluate(variables, functions));

            if (list is EasyScriptList esList)
            {
                if (index < 0 || index >= esList.Count)
                {
                    throw new Exception("List index out of range. Stay within valid bounds!");
                }
                esList[index] = value;
            }
            else
            {
                throw new Exception("Cannot assign to non-list type using index");
            }
        }
        else
        {
            throw new Exception("Invalid assignment target for list");
        }
    }
}