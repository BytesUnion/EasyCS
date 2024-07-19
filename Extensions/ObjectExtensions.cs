public static class ObjectExtensions
{
    public static string getType(this EasyScriptObject obj)
    {
        return "object";
    }
    public static int count(this EasyScriptObject obj)
    {
        return obj.Count;
    }

    public static bool containsKey(this EasyScriptObject obj, string key)
    {
        bool result = obj.ContainsKey(key);
        return result;
    }

    public static object get(this EasyScriptObject obj, string key)
    {
        return obj[key];
    }

    public static void set(this EasyScriptObject obj, string key, object value)
    {
        obj[key] = value;
    }

    public static string asString(this EasyScriptObject obj)
    {
        return obj.ToString();
    }
}