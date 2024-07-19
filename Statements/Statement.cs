abstract class Statement
{
    public int LineNumber { get; set; }
    public abstract void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions);
}