public static class ListExtensions
{
    public static string getType(this EasyScriptList list)
    {
        return "list";
    }
    public static int length(this EasyScriptList list)
    {
        return list.Count;
    }

    public static EasyScriptList reversed(this EasyScriptList list)
    {
        EasyScriptList newList = new EasyScriptList();
        for (int i = list.Count - 1; i >= 0; i--)
        {
            newList.Add(list[i]);
        }
        return newList;
    }

    public static void reverse(this EasyScriptList list)
    {
        EasyScriptList reversedList = reversed(list);
        list.Clear();
        foreach (var item in reversedList)
        {
            list.Add(item);
        }
    }

    public static void add(this EasyScriptList list, object element)
    {
        list.Add(element);
    }

    public static string asString(this EasyScriptList list)
    {
        return list.ToString();
    }
}
