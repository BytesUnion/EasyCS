public static class BoolExtensions
{
    public static string getType(this bool boolean)
    {
        return "bool";
    }
    public static string asString(this bool boolean)
    {
        return boolean.ToString();
    }
}