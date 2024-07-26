class LoadStatement : Statement
{
    public string FileName { get; }
    public string Alias { get; }

    public LoadStatement(string fileName, string alias)
    {
        FileName = fileName;
        Alias = alias;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        try
        {
            string script = File.ReadAllText(FileName);
            Interpreter moduleInterpreter = new Interpreter(FileName);
            moduleInterpreter.Execute(script);

            var moduleObject = new EasyScriptObject();

            foreach (var variable in moduleInterpreter.variables)
            {
                if (moduleInterpreter.SharedVariables.ContainsKey(variable.Key))
                {
                    moduleObject[variable.Key] = variable.Value;
                }
            }

            foreach (var function in moduleInterpreter.functions)
            {
                if (function.Value.IsShared)
                {
                    moduleObject[function.Key] = function.Value;
                }
            }

            variables[Alias] = moduleObject;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error loading file '{FileName}': {ex.Message}");
        }
    }
}