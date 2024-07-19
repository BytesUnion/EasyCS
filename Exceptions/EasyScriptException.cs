public class EasyScriptException : Exception
{
    public EasyScriptException(string message) : base(message)
    {
    }

    public static void ThrowScriptError(string file, int line, string error)
    {
        string errorMessage = $"[EasyScript Error]\nFile: {file}, Line: {line}\n-> {error}";
        Console.WriteLine(errorMessage);
        Environment.Exit(1);
    }
}