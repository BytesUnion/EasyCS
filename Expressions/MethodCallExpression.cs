using System.Reflection;
using System.Runtime.CompilerServices;

class MethodCallExpression : Expression
{
    private Expression target;
    private string methodName;
    private List<Expression> arguments;

    public MethodCallExpression(Expression target, string methodName, List<Expression> arguments)
    {
        this.target = target;
        this.methodName = methodName;
        this.arguments = arguments;
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        object targetObject = target.Evaluate(variables, functions);

        ESConvert converter = new ESConvert();

        if (targetObject is EasyScriptObject eso)
        {
            if (eso.ContainsKey("__class__"))
            {
                return eso.CallMethod(methodName, arguments, variables, functions);
            }
            else if (eso.ContainsKey(methodName))
            {
                object nestedItem = eso[methodName];
                if (nestedItem is EasyScriptClass nestedClass)
                {
                    return nestedClass.Instantiate(arguments, variables, functions);
                }
            }
        }

        MethodInfo methodInfo = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (methodInfo == null)
        {
            methodInfo = GetExtensionMethod(targetObject.GetType(), methodName);
            if (methodInfo == null)
            {
                throw new Exception($"The method '{methodName}' doesn't exist on objects of type '{converter.Convert(targetObject)}'. Check your method name and object type!");
            }
        }

        object[] evaluatedArguments = arguments.Select(arg => arg.Evaluate(variables, functions)).ToArray();

        if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
        {
            var parameters = new object[evaluatedArguments.Length + 1];
            parameters[0] = targetObject;
            Array.Copy(evaluatedArguments, 0, parameters, 1, evaluatedArguments.Length);
            return methodInfo.Invoke(null, parameters);
        }

        return methodInfo.Invoke(targetObject, evaluatedArguments);
    }

    private MethodInfo GetExtensionMethod(Type targetType, string methodName)
    {
        var types = new[] { typeof(ObjectExtensions), typeof(ListExtensions), typeof(StringExtensions), typeof(IntExtensions), typeof(DoubleExtensions), typeof(BoolExtensions) };
        foreach (Type type in types)
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (MethodInfo method in methods)
            {
                if (method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false))
                {
                    if (method.GetParameters()[0].ParameterType.IsAssignableFrom(targetType) && method.Name == methodName)
                    {
                        return method;
                    }
                }
            }
        }
        return null;
    }
}