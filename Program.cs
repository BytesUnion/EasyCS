class Program
{
    static void Main(string[] args)
    {
        if (args.Length < 2 || args[0] != "run")
        {
            Console.WriteLine("[EasyScript Error]");
            Console.WriteLine("-> Usage: easyscript run < script_file >");
            return;
        }

        string fileName = args[1];

        if (!File.Exists(fileName))
        {
            Console.WriteLine("[EasyScript Error]");
            Console.WriteLine($"Error: Script file '{fileName}' not found.");
            return;
        }

        try
        {
            string script = File.ReadAllText(fileName);

            Interpreter interpreter = new Interpreter(fileName);
            interpreter.Execute(script);
        }
        catch (EasyScriptException ex)
        {
            Console.WriteLine($"{ex.GetBaseException()}");
        }
    }
}
