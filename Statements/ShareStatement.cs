class ShareStatement : Statement
{
    public List<string> ItemNames { get; }
    private Dictionary<string, bool> SharedVariables;

    public ShareStatement(List<string> itemNames, Dictionary<string, bool> sharedVariables)
    {
        ItemNames = itemNames;
        SharedVariables = sharedVariables;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        foreach (var itemName in ItemNames)
        {
            if (functions.TryGetValue(itemName, out var function))
            {
                function.IsShared = true;
            }
            else if (variables.ContainsKey(itemName))
            {
                SharedVariables[itemName] = true;
            }
            else
            {
                throw new Exception($"Cannot share '{itemName}': item not found");
            }
        }
    }
}