class ClassStatement : Statement
{
    public string ClassName { get; }
    public string BaseClassName { get; }

    public List<Statement> Body { get; }

    public ClassStatement(string className, string baseClassName, List<Statement> body)
    {
        ClassName = className;
        BaseClassName = baseClassName;
        Body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        EasyScriptClass baseClass = null;

        if (BaseClassName != null)
        {
            if (!variables.TryGetValue(BaseClassName, out var classObj) || !(classObj is EasyScriptClass))
            {
                throw new Exception($"Class '{BaseClassName}' not found");
            }
            baseClass = (EasyScriptClass)classObj;
        }

        variables[ClassName] = new EasyScriptClass(ClassName, baseClass, Body);
    }
}
