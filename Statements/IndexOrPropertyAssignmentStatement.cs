class IndexAssignmentStatement : Statement
{
    public Expression Target { get; }
    public Expression Key { get; }
    public Expression Value { get; }

    public IndexAssignmentStatement(Expression target, Expression key, Expression value)
    {
        Target = target;
        Key = key;
        Value = value;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetValue = Target.Evaluate(variables, functions);
        var keyValue = Key.Evaluate(variables, functions);
        var value = Value.Evaluate(variables, functions);

        if (targetValue is EasyScriptList esList)
        {
            int index = Convert.ToInt32(keyValue);
            if (index < 0 || index >= esList.Count)
            {
                throw new Exception("List index out of range. Stay within valid bounds!");
            }
            esList[index] = value;
        }
        else if (targetValue is EasyScriptObject obj)
        {
            string key = keyValue.ToString();
            obj[key] = value;
        }
        else
        {
            throw new Exception("Cannot assign to non-list or non-object type");
        }
    }
}