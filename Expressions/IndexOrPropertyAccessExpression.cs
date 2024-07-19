class IndexOrPropertyAccessExpression : Expression
{
    public Expression Target { get; }
    public Expression Key { get; }

    public IndexOrPropertyAccessExpression(Expression target, Expression key)
    {
        Target = target;
        Key = key;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetValue = Target.Evaluate(variables, functions);
        var keyValue = Key.Evaluate(variables, functions);

        if (targetValue is EasyScriptObject obj)
        {
            return obj[keyValue.ToString()];
        }

        if (targetValue is EasyScriptList esList)
        {
            int index = Convert.ToInt32(keyValue);
            if (index < 0 || index >= esList.Count)
            {
                throw new Exception("List index out of range. Stay within valid bounds!");
            }
            return esList[index];
        }

        throw new Exception("Invalid target type for property or index access");
    }
}
