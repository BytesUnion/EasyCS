class ClassStatement : Statement
{
    public string ClassName { get; }
    public Expression BaseClassExpression { get; }
    public List<Statement> Body { get; }

    public ClassStatement(string className, Expression baseClassExpression, List<Statement> body)
    {
        ClassName = className;
        BaseClassExpression = baseClassExpression;
        Body = body;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        EasyScriptClass baseClass = null;

        if (BaseClassExpression != null)
        {
            var baseClassObject = BaseClassExpression.Evaluate(variables, functions);
            if (!(baseClassObject is EasyScriptClass))
            {
                throw new Exception($"Base class must be an EasyScriptClass, got {baseClassObject.GetType()}");
            }
            baseClass = (EasyScriptClass)baseClassObject;
        }

        variables[ClassName] = new EasyScriptClass(ClassName, baseClass, Body);
    }
}