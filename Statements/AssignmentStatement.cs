class AssignmentStatement : Statement
{
    public string VariableName { get; }
    public Expression Value { get; }

    public AssignmentStatement(string variableName, Expression value)
    {
        VariableName = variableName;
        Value = value;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        object result = Value.Evaluate(variables, functions);
        if (result is EasyScriptList || result is EasyScriptObject)
        {
            variables[VariableName] = result;
        }
        else
        {
            variables[VariableName] = result;
        }
    }
}