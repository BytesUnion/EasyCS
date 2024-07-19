class ListExpression : Expression
{
    public List<Expression> Elements { get; }

    public ListExpression(List<Expression> elements)
    {
        Elements = elements;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var list = new EasyScriptList();
        foreach (var element in Elements)
        {
            list.Add(element.Evaluate(variables, functions));
        }
        return list;
    }
}