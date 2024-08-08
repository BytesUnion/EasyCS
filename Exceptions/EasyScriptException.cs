public class EasyScriptException : Exception
{
    public EasyScriptException(string message) : base(message) { }

    public static void ThrowScriptError(string fileName, int lineNumber, int column, string script, string message)
    {
        string[] lines = script.Split('\n');

        if (lineNumber - 1 < 0 || lineNumber - 1 >= lines.Length)
        {
            Console.WriteLine("Invalid line number.");
            Environment.Exit(1);
        }

        string errorLine = lines[lineNumber - 1];
        string caretLine = new string(' ', column - 1) + '^';
        string errorMessage = $"[EasyScript Error]\nFile: {fileName}, Position: {lineNumber}:{column}\n\n{errorLine}\n{caretLine}\n\n-> {message}";

        Console.WriteLine(errorMessage);
        Environment.Exit(1);
    }
}
