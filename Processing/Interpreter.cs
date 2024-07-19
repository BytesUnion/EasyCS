using System.Globalization;

class Interpreter
{
    private string currentFile;
    private int currentLine;
    private Dictionary<string, object> variables = new Dictionary<string, object>();
    private Dictionary<string, FunctionStatement> functions = new Dictionary<string, FunctionStatement>();
    private HashSet<string> keywords = new HashSet<string> { "print", "if", "elif", "else", "endf", "True", "False", "f", "endf", "return", "for", "to", "do", "endfor", "while", "endwhile", "in", "break", "class", "endclass", "init", "endinit" };

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
        catch (EasyScriptException)
        {
            throw;
        }
        catch (Exception ex)
        {
            EasyScriptException.ThrowScriptError(currentFile, currentLine, $"Unexpected error: {ex.Message}");
        }
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
                parsedStatement.LineNumber = currentTokenLine;
                statements.Add(parsedStatement);

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
        else if (tokens[i] == "if" || tokens[i] == "elif")
        {
            bool isElseIf = tokens[i] == "elif";
            i++;
            Expect(tokens, ref i, "(");
            Expression condition = ParseExpression(tokens, ref i);
            Expect(tokens, ref i, ")");
            List<Statement> thenBlock = ParseBlock(tokens, ref i, "else", "elif", "endif");
            List<Statement> elseBlock = ParseElseBlock(tokens, ref i);
            return new IfStatement(condition, thenBlock, elseBlock);
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
            return new FunctionStatement(functionName, parameters, body);
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
            string baseClassName = null;
            if (i < tokens.Count && tokens[i] == "extends")
            {
                i++;
                baseClassName = tokens[i++];
            }
            List<Statement> body = ParseBlock(tokens, ref i, "endclass");
            Expect(tokens, ref i, "endclass");
            return new ClassStatement(className, baseClassName, body);
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
        List<Statement> elseBlock = new List<Statement>();
        if (i < tokens.Count && tokens[i] == "else")
        {
            elseBlock.Add(ParseStatement(tokens, ref i));
        }
        else if (i < tokens.Count && tokens[i] == "elif")
        {
            elseBlock.Add(ParseStatement(tokens, ref i));
        }
        return elseBlock;
    }

    private Expression ParseExpression(List<string> tokens, ref int i)
    {
        Expression left = ParseTerm(tokens, ref i);
        while (i < tokens.Count && (tokens[i] == "//" || tokens[i] == "**" || tokens[i] == "+" || tokens[i] == "-" || tokens[i] == ">" || tokens[i] == "<" || tokens[i] == ">=" || tokens[i] == "<=" || tokens[i] == "==" || tokens[i] == "!=" || tokens[i] == "&&" || tokens[i] == "||" || tokens[i] == "in" || tokens[i] == "%"))
        {
            string op = tokens[i++];
            Expression right = ParseTerm(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }

        while (i < tokens.Count && tokens[i] == "[")
        {
            i++;
            Expression key = ParseExpression(tokens, ref i);
            Expect(tokens, ref i, "]");
            left = new IndexOrPropertyAccessExpression(left, key);
        }

        return left;
    }

    private Expression ParseTerm(List<string> tokens, ref int i)
    {
        Expression left = ParseFactor(tokens, ref i);
        while (i < tokens.Count && (tokens[i] == "//" || tokens[i] == "**" || tokens[i] == "*" || tokens[i] == "/" || tokens[i] == "+" || tokens[i] == "-" || tokens[i] == ">" || tokens[i] == "<" || tokens[i] == ">=" || tokens[i] == "<=" || tokens[i] == "==" || tokens[i] == "!=" || tokens[i] == "%"))
        {
            string op = tokens[i++];
            Expression right = ParseFactor(tokens, ref i);
            left = new BinaryExpression(left, op, right);
        }
        return left;
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
            string className = tokens[i++];
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
            result = new NewExpression(className, arguments);
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