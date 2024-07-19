class EasyScriptClass
{
    public string ClassName { get; }
    public EasyScriptClass ParentClass { get; set; }

    public Dictionary<string, InitStatement> Init { get; } = new Dictionary<string, InitStatement>();
    public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();
    public Dictionary<string, FunctionStatement> Methods { get; } = new Dictionary<string, FunctionStatement>();

    public EasyScriptClass(string className, EasyScriptClass baseClassName, List<Statement> body)
    {
        ClassName = className;
        ParentClass = baseClassName;

        if (ParentClass != null)
        {
            foreach (var prop in ParentClass.Properties)
            {
                Properties[prop.Key] = prop.Value;
            }
            foreach (var method in ParentClass.Methods)
            {
                Methods[method.Key] = method.Value;
            }
        }

        foreach (var statement in body)
        {
            if (statement is AssignmentStatement var)
            {
                Properties[var.VariableName] = var.Value.Evaluate(null, null);
            }
            if (statement is ObjectPropertyAssignmentStatement objectPropertyAssignment)
            {
                if (objectPropertyAssignment.Target is VariableExpression variableExpr)
                {
                    Properties[variableExpr.Name] = null;
                }
                else if (objectPropertyAssignment.Target is ObjectPropertyAccessExpression propertyAccessExpr)
                {
                    var key = propertyAccessExpr.Key.Evaluate(null, null).ToString();
                    Properties[key] = null;
                }
                else if (objectPropertyAssignment.Target is IndexOrPropertyAccessExpression indexOrPropertyAccessExpr)
                {
                    var key = indexOrPropertyAccessExpr.Key.Evaluate(null, null).ToString();
                    Properties[key] = null;
                }
            }
            else if (statement is FunctionStatement function)
            {
                Methods[function.Name] = function;
            }
            else if (statement is InitStatement init)
            {
                Init["__init__"] = init;
            }
        }
    }


    public EasyScriptObject Instantiate(List<Expression> arguments, Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var instance = new EasyScriptObject();
        instance["__class__"] = this;
        instance["__parent__"] = ParentClass;

        var currentClass = ParentClass;
        while (currentClass != null)
        {
            foreach (var prop in currentClass.Properties)
            {
                if (!instance.ContainsKey(prop.Key))
                {
                    instance[prop.Key] = prop.Value;
                }
            }
            currentClass = currentClass.ParentClass;
        }

        foreach (var prop in Properties)
        {
            instance[prop.Key] = prop.Value;
        }

        currentClass = ParentClass;
        while (currentClass != null)
        {
            foreach (var method in currentClass.Methods)
            {
                if (!instance.ContainsKey(method.Key))
                {
                    instance[method.Key] = method.Value;
                }
            }
            currentClass = currentClass.ParentClass;
        }

        foreach (var method in Methods)
        {
            instance[method.Key] = method.Value;
        }

        if (Init.TryGetValue("__init__", out var init))
        {
            List<object> evaluatedArguments = arguments.Select(arg => arg.Evaluate(variables, functions)).ToList();
            var initFunction = new InitCallExpression(init, evaluatedArguments, instance);
            initFunction.Evaluate(new Dictionary<string, object>(), functions);
        }

        return instance;
    }



}