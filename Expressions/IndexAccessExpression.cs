class IndexAccessExpression : Expression
{
    public Expression Target { get; }
    public Expression Key { get; }

    public IndexAccessExpression(Expression target, Expression key)
    {
        Target = target;
        Key = key;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var targetValue = Target.Evaluate(variables, functions);
        var keyValue = Key.Evaluate(variables, functions);

        if (targetValue is EasyScriptList esList)
        {
            int index = Convert.ToInt32(keyValue);
            if (index < 0 || index >= esList.Count)
            {
                throw new Exception("List index out of range. Stay within valid bounds!");
            }
            return esList[index];
        }
        else if (targetValue is EasyScriptObject obj)
        {
            string key = keyValue.ToString();
            if (obj.ContainsKey(key))
            {
                return obj[key];
            }
            throw new Exception($"Object does not contain key '{key}'");
        }
        else if (targetValue is string str)
        {
            int index = Convert.ToInt32(keyValue);
            if (index < 0 || index >= str.Length)
            {
                throw new Exception("String index out of range. Stay within valid bounds!");
            }
            return str[index].ToString();
        }
        throw new Exception("Invalid target type for index access.");
    }
}