class ReturnStatement : Statement
{
    private Expression returnValue;
    public ReturnStatement(Expression returnValue)
    {
        this.returnValue = returnValue;
    }
    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        variables["return"] = returnValue.Evaluate(variables, functions);
    }
}