using System.Globalization;

class Interpreter
{
    private string currentFile;
    private int currentLine;
    public Dictionary<string, object> variables = new Dictionary<string, object>();
    public Dictionary<string, FunctionStatement> functions = new Dictionary<string, FunctionStatement>();
    public Dictionary<string, bool> SharedVariables = new Dictionary<string, bool>();
    private Dictionary<string, EasyScriptClass> moduleAliases = new Dictionary<string, EasyScriptClass>();
    private HashSet<string> keywords = new HashSet<string> { "print", "if", "elif", "else", "endf", "True", "False", "f", "endf", "return", "for", "to", "do", "endfor", "while", "endwhile", "in", "break", "class", "endclass", "init", "endinit", "from", "use", "share", "load", "as" };

    public Interpreter(string filename)
    {
        currentFile = filename;
    }

    public void Execute(string script)
    {
        currentLine = 1;
        try
        {
            List<string> tokens = Tokenizer.Tokenize(script);
            List<Statement> statements = Parse(tokens, script);

            foreach (var statement in statements)
            {
                if (statement is ImportStatement importStmt)
                {
                    ImportItems(importStmt.FileName, importStmt.ImportedItems, importStmt.ImportAll);
                }
            }

            foreach (var statement in statements)
            {
                if (!(statement is ImportStatement))
                {
                    try
                    {
                        statement.Execute(variables, functions);
                    }
                    catch (Exception ex)
                    {
                        EasyScriptException.ThrowScriptError(currentFile, statement.LineNumber, ex.Message);
                    }
                }
            }
        }
        catch (EasyScriptException)
        {
            throw;
        }
        catch (Exception ex)
        {
            EasyScriptException.ThrowScriptError(currentFile, currentLine, $"Unexpected error: {ex.Message}");
        }
    }

    private void ImportItems(string fileName, List<string> importedItems, bool importAll)
    {
        try
        {
            string script = File.ReadAllText(fileName);
            Interpreter moduleInterpreter = new Interpreter(fileName);
            moduleInterpreter.Execute(script);

            foreach (var pair in moduleInterpreter.variables)
            {
                if (importAll || importedItems.Contains(pair.Key))
                {
                    if (moduleInterpreter.SharedVariables.ContainsKey(pair.Key))
                    {
                        if (pair.Value is EasyScriptClass classObj)
                        {
                            variables[pair.Key] = new EasyScriptClass(classObj.ClassName, classObj.ParentClass, GetClassBody(classObj));
                        }
                        else
                        {
                            variables[pair.Key] = pair.Value;
                        }
                    }
                }
            }

            foreach (var pair in moduleInterpreter.functions)
            {
                if ((importAll || importedItems.Contains(pair.Key)) && pair.Value.IsShared)
                {
                    functions[pair.Key] = pair.Value;
                }
            }

            if (!importAll)
            {
                foreach (var itemName in importedItems)
                {
                    if (!variables.ContainsKey(itemName) && !functions.ContainsKey(itemName))
                    {
                        throw new Exception($"Item '{itemName}' not found or not shared in module '{fileName}'");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error importing from '{fileName}': {ex.Message}");
        }
    }


    private List<Statement> GetClassBody(EasyScriptClass classObj)
    {
        var body = new List<Statement>();

        foreach (var prop in classObj.Properties)
        {
            body.Add(new AssignmentStatement(prop.Key, new ConstantExpression(prop.Value)));
        }

        foreach (var method in classObj.Methods)
        {
            body.Add(method.Value);
        }

        if (classObj.Init.TryGetValue("__init__", out var init))
        {
            body.Add(init);
        }

        return body;
    }

    private List<Statement> Parse(List<string> tokens, string originalScript)
    {
        List<Statement> statements = new List<Statement>();
        int i = 0;
        string[] lines = originalScript.Split('\n');
        int currentTokenLine = 1;

        while (i < tokens.Count)
        {
            try
            {
                Statement parsedStatement = ParseStatement(tokens, ref i);
                if (parsedStatement != null)
                {
                    parsedStatement.LineNumber = currentTokenLine;
                    statements.Add(parsedStatement);
                }

                while (currentTokenLine < lines.Length && i < tokens.Count &&
                       originalScript.Substring(0, originalScript.IndexOf(tokens[i])).Split('\n').Length > currentTokenLine)
                {
                    currentTokenLine++;
                }
            }
            catch (Exception ex)
            {
                EasyScriptException.ThrowScriptError(currentFile, currentTokenLine, ex.Message);
            }
        }
        return statements;
    }

    private Statement ParseStatement(List<string> tokens, ref int i)
    {
        currentLine++;
        if (i < tokens.Count && tokens[i].StartsWith(">>"))
        {
            i++;
            return null;
        }

        if (tokens[i] == "break")
        {
            i++;
            return new BreakStatement();
        }
        else if (tokens[i] == "print")
        {
            i++;
            Expect(tokens, ref i, "(");
            Expression expr = ParseExpression(tokens, ref i);
            Expect(tokens, ref i, ")");
            return new PrintStatement(expr);
        }
        else if (tokens[i] == "if")
        {
            return ParseIfStatement(tokens, ref i);
        }
        else if (tokens[i] == "else")
        {
            i++;
            List<Statement> elseBlock = ParseBlock(tokens, ref i, "endif");
            return new ElseStatement(elseBlock);
        }
        else if (tokens[i] == "endif")
        {
            i++;
            return new EndIfStatement();
        }
        else if (tokens[i] == "f")
        {
            i++;
            string functionName = tokens[i++];
            List<string> parameters = new List<string>();
            Expect(tokens, ref i, "(");
            while (tokens[i] != ")")
            {
                parameters.Add(tokens[i++]);
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            List<Statement> body = ParseBlock(tokens, ref i, "endf");
            Expect(tokens, ref i, "endf");
            return new FunctionStatement(functionName, parameters, body, new Dictionary<string, object>(variables));
        }
        else if (tokens[i] == "init")
        {
            i++;
            List<string> parameters = new List<string>();
            Expect(tokens, ref i, "(");
            while (tokens[i] != ")")
            {
                parameters.Add(tokens[i++]);
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            List<Statement> body = ParseBlock(tokens, ref i, "endinit");
            Expect(tokens, ref i, "endinit");
            return new InitStatement(parameters, body);
        }
        else if (tokens[i] == "super")
        {
            i++;
            Expect(tokens, ref i, "(");
            List<Expression> arguments = new List<Expression>();
            while (tokens[i] != ")")
            {
                arguments.Add(ParseExpression(tokens, ref i));
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            return new SuperMethodCallStatement("__init__", arguments);
        }
        else if (tokens[i] == "return")
        {
            i++;
            Expression returnValue = ParseExpression(tokens, ref i);
            return new ReturnStatement(returnValue);
        }
        else if (tokens[i] == "while")
        {
            i++;
            Expect(tokens, ref i, "(");
            Expression condition = ParseExpression(tokens, ref i);
            Expect(tokens, ref i, ")");
            Expect(tokens, ref i, "do");
            List<Statement> body = ParseBlock(tokens, ref i, "endwhile");
            Expect(tokens, ref i, "endwhile");
            return new WhileStatement(condition, body);
        }
        else if (tokens[i] == "for")
        {
            i++;
            Expect(tokens, ref i, "(");
            if (tokens[i + 1] == "in")
            {
                string variable = tokens[i++];
                Expect(tokens, ref i, "in");
                Expression iterableExpression = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, ")");
                Expect(tokens, ref i, "do");
                List<Statement> body = ParseBlock(tokens, ref i, "endfor");
                Expect(tokens, ref i, "endfor");
                return new ForEachStatement(variable, iterableExpression, body);
            }
            else if (tokens[i + 1] == "=")
            {
                string variable = tokens[i++];
                Expect(tokens, ref i, "=");
                Expression startValue = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, "to");
                Expression endValue = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, ")");
                Expect(tokens, ref i, "do");
                List<Statement> body = ParseBlock(tokens, ref i, "endfor");
                Expect(tokens, ref i, "endfor");
                return new ForStatement(variable, startValue, endValue, body);
            }
            else if (tokens[i] == "(")
            {
                i++;
                string firstVariable = tokens[i++];
                Expect(tokens, ref i, ",");
                string secondVariable = tokens[i++];
                Expect(tokens, ref i, ")");
                Expect(tokens, ref i, "in");
                Expression iterableExpression = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, ")");
                Expect(tokens, ref i, "do");
                List<Statement> body = ParseBlock(tokens, ref i, "endfor");
                Expect(tokens, ref i, "endfor");
                return new ForEachKVStatement(firstVariable, secondVariable, iterableExpression, body);
            }
        }
        else if (tokens[i] == "class")
        {
            i++;
            string className = tokens[i++];
            Expression baseClassExpression = null;
            if (i < tokens.Count && tokens[i] == "extends")
            {
                i++;
                baseClassExpression = ParseExpression(tokens, ref i);
            }
            List<Statement> body = ParseBlock(tokens, ref i, "endclass");
            Expect(tokens, ref i, "endclass");
            return new ClassStatement(className, baseClassExpression, body);
        }
        else if (tokens[i] == "share")
        {
            i++;
            Expect(tokens, ref i, "(");
            List<string> importedItems = new List<string>();
            while (i < tokens.Count && tokens[i] != ")")
            {
                importedItems.Add(tokens[i++]);
                if (i < tokens.Count && tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            return new ShareStatement(importedItems, SharedVariables);
        }
        else if (tokens[i] == "from")
        {
            i++;
            string fileName = tokens[i++].Trim('"');
            Expect(tokens, ref i, "use");

            if (tokens[i] == "*")
            {
                i++;
                return new ImportStatement(fileName, new List<string>(), true);
            }
            else
            {
                Expect(tokens, ref i, "{");
                List<string> importedItems = new List<string>();
                while (i < tokens.Count && tokens[i] != "}")
                {
                    importedItems.Add(tokens[i++]);
                    if (i < tokens.Count && tokens[i] == ",")
                    {
                        i++;
                    }
                }
                Expect(tokens, ref i, "}");
                return new ImportStatement(fileName, importedItems, false);
            }
        }
        else if (tokens[i] == "load")
        {
            i++;
            string fileName = tokens[i++].Trim('"');
            Expect(tokens, ref i, "as");
            string alias = tokens[i++];
            return new LoadStatement(fileName, alias);
        }
        else if (char.IsLetter(tokens[i][0]))
        {
            string identifier = tokens[i++];
            Expression target = new VariableExpression(identifier);

            while (i < tokens.Count && (tokens[i] == "[" || tokens[i] == "."))
            {
                if (tokens[i] == "[")
                {
                    i++;
                    Expression key = ParseExpression(tokens, ref i);
                    Expect(tokens, ref i, "]");
                    target = new IndexAccessExpression(target, key);
                }
                else if (tokens[i] == ".")
                {
                    i++;
                    string propertyName = tokens[i++];

                    if (i < tokens.Count && tokens[i] == "(")
                    {
                        i++;
                        List<Expression> arguments = new List<Expression>();
                        while (tokens[i] != ")")
                        {
                            arguments.Add(ParseExpression(tokens, ref i));
                            if (tokens[i] == ",")
                            {
                                i++;
                            }
                        }
                        Expect(tokens, ref i, ")");
                        return new MethodCallStatement(identifier, propertyName, arguments);
                    }
                    else
                    {
                        target = new IndexAccessExpression(target, new StringExpression(propertyName));
                    }
                }
            }

            if (tokens[i] == "=")
            {
                i++;
                Expression value = ParseExpression(tokens, ref i);
                if (target is VariableExpression varExpr)
                {
                    return new AssignmentStatement(varExpr.Name, value);
                }
                else if (target is IndexAccessExpression indexExpr)
                {
                    return new IndexAssignmentStatement(indexExpr.Target, indexExpr.Key, value);
                }
                else
                {
                    throw new Exception("Invalid assignment target");
                }
            }
            else if (tokens[i] == "(")
            {
                i++;
                List<Expression> arguments = new List<Expression>();
                while (tokens[i] != ")")
                {
                    arguments.Add(ParseExpression(tokens, ref i));
                    if (tokens[i] == ",")
                    {
                        i++;
                    }
                }
                Expect(tokens, ref i, ")");
                return new FunctionCallStatement(identifier, arguments);
            }
            else if (tokens[i] == ".")
            {
                i++;
                string methodName = tokens[i++];
                Expect(tokens, ref i, "(");
                List<Expression> arguments = new List<Expression>();
                while (tokens[i] != ")")
                {
                    arguments.Add(ParseExpression(tokens, ref i));
                    if (tokens[i] == ",")
                    {
                        i++;
                    }
                }
                Expect(tokens, ref i, ")");
                return new MethodCallStatement(identifier, methodName, arguments);
            }
            else
            {
                throw new Exception($"Unexpected token after identifier '{identifier}': '{tokens[i]}'");
            }
        }
        throw new Exception($"Invalid statement starting with '{tokens[i]}'");
    }

    private List<Statement> ParseBlock(List<string> tokens, ref int i, params string[] terminators)
    {
        List<Statement> block = new List<Statement>();
        while (i < tokens.Count && !terminators.Contains(tokens[i]))
        {
            block.Add(ParseStatement(tokens, ref i));
        }
        return block;
    }

    private List<Statement> ParseElseBlock(List<string> tokens, ref int i)
    {
        if (i < tokens.Count && (tokens[i] == "else" || tokens[i] == "elif"))
        {
            return ParseBlock(tokens, ref i, "endif");
        }
        return null;
    }


    private Expression ParseExpression(List<string> tokens, ref int i)
    {
        return ParseLogicalOr(tokens, ref i);
    }

    private Expression ParseLogicalOr(List<string> tokens, ref int i)
    {
        Expression left = ParseLogicalAnd(tokens, ref i);
        while (i < tokens.Count && tokens[i] == "||")
        {
            string op = tokens[i++];
            Expression right = ParseLogicalAnd(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseLogicalAnd(List<string> tokens, ref int i)
    {
        Expression left = ParseEquality(tokens, ref i);
        while (i < tokens.Count && tokens[i] == "&&")
        {
            string op = tokens[i++];
            Expression right = ParseEquality(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseEquality(List<string> tokens, ref int i)
    {
        Expression left = ParseRelational(tokens, ref i);
        while (i < tokens.Count && (tokens[i] == "==" || tokens[i] == "!="))
        {
            string op = tokens[i++];
            Expression right = ParseRelational(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseRelational(List<string> tokens, ref int i)
    {
        Expression left = ParseArithmetic(tokens, ref i);
        while (i < tokens.Count && (tokens[i] == ">" || tokens[i] == "<" || tokens[i] == ">=" || tokens[i] == "<="))
        {
            string op = tokens[i++];
            Expression right = ParseArithmetic(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseArithmetic(List<string> tokens, ref int i)
    {
        Expression left = ParseTerm(tokens, ref i);

        while (i < tokens.Count && (tokens[i] == "+" || tokens[i] == "-"))
        {
            string op = tokens[i++];
            Expression right = ParseTerm(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }

        return left;
    }

    private Expression ParseTerm(List<string> tokens, ref int i)
    {
        Expression left = ParsePower(tokens, ref i);
        while (i < tokens.Count && (tokens[i] == "*" || tokens[i] == "/" || tokens[i] == "%" || tokens[i] == "//"))
        {
            string op = tokens[i++];
            Expression right = ParsePower(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParsePower(List<string> tokens, ref int i)
    {
        Expression left = ParseFactor(tokens, ref i);
        while (i < tokens.Count && tokens[i] == "**")
        {
            string op = tokens[i++];
            Expression right = ParseFactor(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Statement ParseIfStatement(List<string> tokens, ref int i)
    {
        i++;
        Expect(tokens, ref i, "(");
        Expression condition = ParseExpression(tokens, ref i);
        Expect(tokens, ref i, ")");

        List<Statement> thenBlock = ParseBlock(tokens, ref i, "elif", "else", "endif");
        List<IfStatement> elifStatements = new List<IfStatement>();
        List<Statement> elseBlock = null;

        while (i < tokens.Count)
        {
            if (tokens[i] == "elif")
            {
                i++;
                Expect(tokens, ref i, "(");
                Expression elifCondition = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, ")");
                List<Statement> elifBlock = ParseBlock(tokens, ref i, "elif", "else", "endif");
                elifStatements.Add(new IfStatement(elifCondition, elifBlock, null, new List<IfStatement>()));
            }
            else if (tokens[i] == "else")
            {
                i++;
                elseBlock = ParseBlock(tokens, ref i, "endif");
                break;
            }
            else if (tokens[i] == "endif")
            {
                i++;
                break;
            }
            else
            {
                throw new Exception($"Unexpected token '{tokens[i]}' in if statement");
            }
        }

        return new IfStatement(condition, thenBlock, elseBlock, elifStatements);
    }

    private Expression ParseFactor(List<string> tokens, ref int i)
    {
        Expression result;

        if (tokens[i] == "-")
        {
            i++;
            Expression factor = ParseFactor(tokens, ref i);
            result = new UnaryExpression("-", factor);
        }
        else if (int.TryParse(tokens[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
        {
            i++;
            result = new ConstantExpression(intValue);
        }
        else if (double.TryParse(tokens[i], NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
        {
            i++;
            result = new ConstantExpression(doubleValue);
        }
        else if (tokens[i] == "True" || tokens[i] == "False")
        {
            bool boolValue = tokens[i] == "True";
            i++;
            result = new BooleanExpression(boolValue);
        }
        else if ((tokens[i][0] == '"' && tokens[i][tokens[i].Length - 1] == '"') || (tokens[i][0] == '\'' && tokens[i][tokens[i].Length - 1] == '\''))
        {
            string stringValue = tokens[i++];
            result = new StringExpression(stringValue.Substring(1, stringValue.Length - 2));
        }
        else if (tokens[i] == "null")
        {
            i++;
            result = new NullExpression();
        }
        else if (tokens[i] == "[")
        {
            i++;
            var elements = new List<Expression>();
            while (tokens[i] != "]")
            {
                elements.Add(ParseExpression(tokens, ref i));
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, "]");
            result = new ListExpression(elements);
        }
        else if (tokens[i] == "new")
        {
            i++;
            var classPath = new List<string>();
            classPath.Add(tokens[i++]);

            while (i < tokens.Count && tokens[i] == ".")
            {
                i++;
                classPath.Add(tokens[i++]);
            }

            Expect(tokens, ref i, "(");
            var arguments = new List<Expression>();
            while (tokens[i] != ")")
            {
                Expression parsed = ParseExpression(tokens, ref i);
                arguments.Add(parsed);
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            result = new NewExpression(classPath, arguments);
        }
        else if (tokens[i] == "{")
        {
            i++;
            var properties = new Dictionary<string, Expression>();
            while (tokens[i] != "}")
            {
                string key = tokens[i++];
                Expect(tokens, ref i, ":");
                Expression value = ParseExpression(tokens, ref i);
                properties.Add(key, value);
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, "}");
            result = new ObjectExpression(properties);
        }
        else if (char.IsLetter(tokens[i][0]))
        {
            string identifier = tokens[i++];
            Expression expr = new VariableExpression(identifier);

            if (i < tokens.Count && tokens[i] == "(")
            {
                i++;
                List<Expression> arguments = new List<Expression>();
                while (tokens[i] != ")")
                {
                    arguments.Add(ParseExpression(tokens, ref i));
                    if (tokens[i] == ",")
                    {
                        i++;
                    }
                }
                Expect(tokens, ref i, ")");
                expr = new FunctionCallExpression(identifier, arguments);
            }

            return ParseMethodCallOrPropertyAccess(tokens, ref i, expr);
        }
        else if (tokens[i] == "(")
        {
            i++;
            result = ParseExpression(tokens, ref i);
            Expect(tokens, ref i, ")");
        }
        else
        {
            throw new Exception($"Unexpected token '{tokens[i]}'");
        }

        return ParseMethodCallOrPropertyAccess(tokens, ref i, result);
    }

    private Expression ParseMethodCallOrPropertyAccess(List<string> tokens, ref int i, Expression target)
    {
        while (i < tokens.Count && (tokens[i] == "." || tokens[i] == "["))
        {
            if (tokens[i] == ".")
            {
                i++;
                string memberName = tokens[i++];
                if (i < tokens.Count && tokens[i] == "(")
                {
                    i++;
                    var arguments = new List<Expression>();
                    while (tokens[i] != ")")
                    {
                        arguments.Add(ParseExpression(tokens, ref i));
                        if (tokens[i] == ",")
                        {
                            i++;
                        }
                    }
                    Expect(tokens, ref i, ")");
                    target = new MethodCallExpression(target, memberName, arguments);
                }
                else
                {
                    target = new PropertyAccessExpression(target, memberName);
                }
            }
            else if (tokens[i] == "[")
            {
                i++;
                Expression index = ParseExpression(tokens, ref i);
                Expect(tokens, ref i, "]");
                target = new IndexAccessExpression(target, index);
            }
        }
        return target;
    }



    private Expression ParseMethodCall(List<string> tokens, ref int i, Expression target)
    {
        while (i < tokens.Count && tokens[i] == ".")
        {
            i++;
            string methodName = tokens[i++];
            Expect(tokens, ref i, "(");
            var arguments = new List<Expression>();
            while (tokens[i] != ")")
            {
                arguments.Add(ParseExpression(tokens, ref i));
                if (tokens[i] == ",")
                {
                    i++;
                }
            }
            Expect(tokens, ref i, ")");
            target = new MethodCallExpression(target, methodName, arguments);
        }
        return target;
    }

    private Expression ParsePropertyAccess(List<string> tokens, ref int i, Expression target)
    {
        while (i < tokens.Count && tokens[i] == ".")
        {
            i++;
            string propertyName = tokens[i++];
            target = new PropertyAccessExpression(target, propertyName);
        }
        return target;
    }

    private Expression GetRootTarget(IndexAccessExpression expr)
    {
        while (expr.Target is IndexAccessExpression nestedExpr)
        {
            expr = nestedExpr;
        }
        return expr.Target;
    }


    private void Expect(List<string> tokens, ref int i, string expected)
    {
        if (tokens[i] != expected)
        {
            throw new Exception($"Expected '{expected}', but found '{tokens[i]}'");
        }
        i++;
    }
}