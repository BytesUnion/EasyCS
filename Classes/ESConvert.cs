public class ESConvert
{
    public string Convert(object convertation)
    {
        Type type = convertation.GetType();
        switch (type.Name)
        {
            case "Double":
                return "double";
            case "Int32":
                return "int";
            case "String":
                if (int.TryParse(convertation.ToString(), out _) == true) return "int";
                if (double.TryParse(convertation.ToString(), out _) == true) return "double";
                return "string";
            case "EasyScriptList":
                return "list";
            case "EasyScriptObject":
                return "object";
            case "EasyScriptClass":
                return "class";
            case "Boolean":
                return "boolean";
            default:
                throw new Exception($"Unsupported type: {type.Name}");
        }
    }
}
