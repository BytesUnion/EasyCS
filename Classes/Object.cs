public class EasyScriptObjectList
{
    private string[] keys;
    private object[] values;
    private int count;

    public EasyScriptObjectList()
    {
        keys = new string[4];
        values = new object[4];
        count = 0;
    }

    public int Count => count;

    public void Add(string key, object value)
    {
        if (count == keys.Length)
        {
            Resize();
        }

        key = key.Trim('"');
        keys[count] = key;
        values[count++] = value;
    }

    public object this[string key]
    {
        get
        {
            int index = FindIndex(key);
            if (index == -1)
            {
                throw new KeyNotFoundException($"Key '{key}' not found.");
            }
            return values[index];
        }
        set
        {
            int index = FindIndex(key);
            if (index == -1)
            {
                Add(key, value);
            }
            else
            {
                values[index] = value;
            }
        }
    }

    public bool ContainsKey(string key)
    {
        return FindIndex(key) != -1;
    }

    public string GetKeyAt(int index)
    {
        if (index < 0 || index >= count)
        {
            throw new IndexOutOfRangeException($"Index '{index}' is out of range.");
        }
        return keys[index];
    }

    private int FindIndex(string key)
    {
        key = key.Trim('"');
        for (int i = 0; i < count; i++)
        {
            if (string.Equals(keys[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    public override string ToString()
    {
        var keyValuePairs = keys.Take(count).Zip(values.Take(count), (k, v) =>
            $"\"{k}\": {(v is string ? $"\"{v}\"" : v)}");
        return "{" + string.Join(", ", keyValuePairs) + "}";
    }

    private void Resize()
    {
        int newSize = keys.Length * 2;
        Array.Resize(ref keys, newSize);
        Array.Resize(ref values, newSize);
    }
}

public class EasyScriptObject
{
    private EasyScriptObjectList properties;

    public EasyScriptObject()
    {
        properties = new EasyScriptObjectList();
    }

    public int Count => properties.Count;

    public bool ContainsKey(string key)
    {
        return properties.ContainsKey(key);
    }

    public string GetKeyAt(int index)
    {
        if (index < 0 || index >= Count)
        {
            throw new IndexOutOfRangeException($"Index '{index}' is out of range.");
        }
        return properties.GetKeyAt(index);
    }

    public object this[string key]
    {
        get => properties[key];
        set => properties[key] = value;
    }

    public void Add(string key, object value)
    {
        properties.Add(key, value);
    }

    public object GetProperty(string propertyName)
    {
        if (this.ContainsKey(propertyName))
        {
            return this[propertyName];
        }
        throw new Exception($"Property '{propertyName}' not found");
    }

    public override string ToString()
    {
        return properties.ToString();
    }

    public Dictionary<string, object> ToDictionary()
    {
        var dict = new Dictionary<string, object>();
        for (int i = 0; i < properties.Count; i++)
        {
            string key = properties.GetKeyAt(i);
            dict[key] = properties[key];
        }
        return dict;
    }

    internal object CallMethod(string methodName, List<Expression> arguments, Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        if (this.ContainsKey("__class__"))
        {
            var classObj = (EasyScriptClass)this["__class__"];
            while (classObj != null)
            {
                if (classObj.Methods.TryGetValue(methodName, out var method))
                {
                    var localVars = new Dictionary<string, object>(variables);
                    localVars["cur"] = this;

                    foreach (var arg in method.Parameters.Zip(arguments, (param, arg) => new { param, arg }))
                    {
                        localVars[arg.param] = arg.arg.Evaluate(localVars, functions);
                    }

                    object returnValue = null;
                    foreach (var statement in method.Body)
                    {
                        statement.Execute(localVars, functions);
                        if (localVars.ContainsKey("return"))
                        {
                            returnValue = localVars["return"];
                            localVars.Remove("return");
                            break;
                        }
                    }
                    return returnValue;
                }
                classObj = classObj.ParentClass;
            }
        }
        throw new Exception($"Method '{methodName}' is not supported on object of type '{this.GetType().Name}'.");
    }

}
