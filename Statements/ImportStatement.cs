class ImportStatement : Statement
{
    public string FileName { get; }
    public List<string> ImportedItems { get; }
    public bool ImportAll { get; }

    public ImportStatement(string fileName, List<string> importedItems, bool importAll)
    {
        FileName = fileName;
        ImportedItems = importedItems;
        ImportAll = importAll;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
    }
}