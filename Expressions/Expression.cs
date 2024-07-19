abstract class Expression
{
    public int LineNumber { get; set; }
    public abstract object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions);
}