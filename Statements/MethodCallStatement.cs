using System.Text;

class MethodCallStatement : Statement
{
    private string variable;
    private string methodName;
    private List<Expression> arguments;

    public MethodCallStatement(string variable, string methodName, List<Expression> arguments)
    {
        this.variable = variable;
        this.methodName = methodName;
        this.arguments = arguments;
    }

    public override void Execute(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        ESConvert converter = new ESConvert();
        object instance = variables[variable];

        if (instance is string str)
        {
            if (methodName == "reverse")
            {
                variables[variable] = StringExtensions.reversed(str);
                return;
            }
            else if (methodName == "reversed")
            {
                variables["return"] = StringExtensions.reversed(str);
                return;
            }
        }
        else if (instance is StringBuilder sb)
        {
            if (methodName == "reverse")
            {
                sb.reverse();
                variables[variable] = sb;
                return;
            }
        }
        else if (instance is EasyScriptObject eso)
        {
            if (eso.containsKey("__class__"))
            {
                var result = eso.CallMethod(methodName, arguments, variables, functions);
                if (result != null)
                {
                    variables["return"] = result;
                }
                return;
            }
            if (methodName == "set")
            {
                if (arguments[0] == null)
                {
                    throw new Exception("Missing parameter 'key'");
                }
                if (arguments[1] == null)
                {
                    throw new Exception("Missing parameter 'value'");
                }
                string key = arguments[0].Evaluate(variables, functions).ToString();
                object value = arguments[1].Evaluate(variables, functions);
                ObjectExtensions.set(eso, key, value);
                return;
            }
        }
        else if (instance is EasyScriptList esl)
        {
            if (methodName == "reverse")
            {
                ListExtensions.reverse(esl);
                variables[variable] = esl;
                return;
            }
            else if (methodName == "add")
            {
                if (arguments[0] == null)
                {
                    throw new Exception("Missing parameter 'element'");
                }
                ListExtensions.add(esl, arguments[0].Evaluate(variables, functions));
                return;
            }
        }

        throw new Exception($"Method '{methodName}' not supported on object of type '{converter.Convert(instance)}'.");
    }
}
