using System.Globalization;
using System.Text;

public static class StringExtensions
{
    public static string getType(this string str)
    {
        return "string";
    }

    public static double length(this string str)
    {
        return str.Length;
    }

    public static double asDouble(this string str)
    {
        if (double.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }
        throw new Exception($"Cannot convert '{str}' to double.");
    }

    public static int asInt(this string str)
    {
        if (int.TryParse(str, out int result))
        {
            return result;
        }
        throw new Exception($"Cannot convert '{str}' to int.");
    }

    public static string reversed(this string str)
    {
        char[] charArray = str.ToCharArray();
        Array.Reverse(charArray);
        return new string(charArray);
    }

    public static void reverse(this StringBuilder str)
    {
        char[] charArray = str.ToString().ToCharArray();
        Array.Reverse(charArray);
        str.Clear();
        str.Append(new string(charArray));
    }

    public static string uppercase(this string str)
    {
        return str.ToUpper();
    }

    public static string lowercase(this string str)
    {
        return str.ToLower();
    }

    public static string asString(this string str)
    {
        return str.ToString(); 
    }
}