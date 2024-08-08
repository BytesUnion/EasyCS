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

        if (targetObject is EasyScriptObject eso)
        {

            if (eso.ContainsKey("__class__"))
            {
                return eso.CallMethod(methodName, arguments, variables, functions);
            } else if (eso.ContainsKey(methodName))
            {
                object method = eso[methodName];
                if (method is FunctionStatement function)
                {
                    var localVariables = new Dictionary<string, object>(variables);
                    var evaluatedArgs = arguments.Select(arg => arg.Evaluate(variables, functions)).ToArray();

                    for (int i = 0; i < function.Parameters.Count; i++)
                    {
                        localVariables[function.Parameters[i]] = evaluatedArgs[i];
                    }

                    foreach (var statement in function.Body)
                    {
                        statement.Execute(localVariables, functions);
                        if (localVariables.ContainsKey("return"))
                        {
                            return localVariables["return"];
                        }
                    }
                    return null;
                } 
                else
                {
                    throw new Exception($"The method '{methodName}' is not a valid function.");
                }
            }
        }

        MethodInfo methodInfo = targetObject.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (methodInfo == null)
        {
            methodInfo = GetExtensionMethod(targetObject.GetType(), methodName);
            if (methodInfo == null)
            {
                throw new Exception($"The method '{methodName}' doesn't exist on objects of type '{targetObject.GetType().Name}'. Check your method name and object type!");
            }
        }

        object[] args = arguments.Select(arg => arg.Evaluate(variables, functions)).ToArray();
        if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
        {
            var parameters = new object[args.Length + 1];
            parameters[0] = targetObject;
            Array.Copy(args, 0, parameters, 1, args.Length);
            return methodInfo.Invoke(null, parameters);
        }

        return methodInfo.Invoke(targetObject, args);
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
