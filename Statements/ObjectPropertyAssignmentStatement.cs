class ObjectPropertyAssignmentStatement : Statement
{
    public Expression Target { get; }
    public Expression Key { get; }
    public Expression Value { get; }

    public ObjectPropertyAssignmentStatement(Expression target, Expression key, Expression value)
    {
        Target = target;
        Key = key;
        Value = value;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetObject = Target.Evaluate(variables, functions);
        if (targetObject is EasyScriptObject obj)
        {
            var key = Key.Evaluate(variables, functions).ToString();
            obj[key] = Value.Evaluate(variables, functions);
        }
        else
        {
            throw new Exception("Cannot assign property to non-object type");
        }
    }
}