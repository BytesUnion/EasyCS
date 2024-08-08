using System.Globalization;

class Interpreter
{
    private string currentFile;
    private List<Token> tokens;
    private int currentTokenIndex;
    public Dictionary<string, object> variables = new Dictionary<string, object>();
    public Dictionary<string, FunctionStatement> functions = new Dictionary<string, FunctionStatement>();
    public Dictionary<string, bool> SharedVariables = new Dictionary<string, bool>();
    private Dictionary<string, EasyScriptClass> moduleAliases = new Dictionary<string, EasyScriptClass>();
    private string script;

    public Interpreter(string filename)
    {
        currentFile = filename;
    }

    public void Execute(string script)
    {
        try
        {
            Lexer lexer = new Lexer(script);
            tokens = lexer.TokenizeAll();
            currentTokenIndex = 0;

            List<Statement> statements = Parse();

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
                        EasyScriptException.ThrowScriptError(currentFile, statement.LineNumber, statement.Column, script, ex.Message);
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
            Console.WriteLine(ex.ToString());
            EasyScriptException.ThrowScriptError(currentFile, CurrentToken.Line, CurrentToken.Column, script, $"Unexpected error: {ex.Message}");
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
                    variables[pair.Key] = pair.Value;
                }
            }

            foreach (var pair in moduleInterpreter.functions)
            {
                if (importAll || importedItems.Contains(pair.Key))
                {
                    functions[pair.Key] = pair.Value;
                    if (moduleInterpreter.SharedVariables.ContainsKey(pair.Key))
                    {
                        SharedVariables[pair.Key] = true;
                    }
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
            if (functions.ContainsKey("__init__") && functions["__init__"].IsShared)
            {
                init.IsShared = true;
            }
            body.Add(init);
        }

        return body;
    }

    private List<Statement> Parse()
    {
        List<Statement> statements = new List<Statement>();

        while (currentTokenIndex < tokens.Count && CurrentToken.Type != TokenType.EOF)
        {
            try
            {
                Statement parsedStatement = ParseStatement();
                if (parsedStatement != null)
                {
                    parsedStatement.LineNumber = CurrentToken.Line;
                    parsedStatement.Column = CurrentToken.Column;
                    statements.Add(parsedStatement);
                }
            }
            catch (Exception ex)
            {
                EasyScriptException.ThrowScriptError(currentFile, CurrentToken.Line, CurrentToken.Column, script, ex.Message);
            }
        }
        return statements;
    }


    private Statement ParseStatement()
    {
        try
        {
            if (Match(TokenType.Comment))
            {
                return null;
            }

            if (Match(TokenType.Keyword))
            {
                switch (PreviousToken.Value)
                {
                    case "break": return new BreakStatement();
                    case "print": return ParsePrintStatement();
                    case "if": return ParseIfStatement();
                    case "f": return ParseFunctionStatement();
                    case "init": return ParseInitStatement();
                    case "return": return ParseReturnStatement();
                    case "while": return ParseWhileStatement();
                    case "for": return ParseForStatement();
                    case "class": return ParseClassStatement();
                    case "share": return ParseShareStatement();
                    case "from": return ParseImportStatement();
                    case "super": return ParseSuperStatement();
                    case "load": return ParseLoadStatement();
                }
            }

            if (Match(TokenType.Identifier))
            {
                return ParseAssignmentOrFunctionCall();
            }

            throw new Exception($"Unexpected token: {CurrentToken.Value}");
        }
        catch (Exception ex)
        {
            EasyScriptException.ThrowScriptError(currentFile, CurrentToken.Line, CurrentToken.Column, script, ex.Message);
            return null;
        }
    }


    private Statement ParsePrintStatement()
    {
        Expect(TokenType.Delimiter, "(");
        Expression expr = ParseExpression();
        Expect(TokenType.Delimiter, ")");
        return new PrintStatement(expr);
    }

    private Statement ParseIfStatement()
    {
        Expect(TokenType.Delimiter, "(");
        Expression condition = ParseExpression();
        Expect(TokenType.Delimiter, ")");

        List<Statement> thenBlock = new List<Statement>();
        List<IfStatement> elifStatements = new List<IfStatement>();
        List<Statement> elseBlock = null;

        while (!Match(TokenType.Keyword, "endif"))
        {
            if (Match(TokenType.Keyword, "elif"))
            {
                Expect(TokenType.Delimiter, "(");
                Expression elifCondition = ParseExpression();
                Expect(TokenType.Delimiter, ")");
                List<Statement> elifBlock = ParseBlock("elif", "else", "endif");
                elifStatements.Add(new IfStatement(elifCondition, elifBlock, null, new List<IfStatement>()));
            }
            else if (Match(TokenType.Keyword, "else"))
            {
                elseBlock = ParseBlock("endif");
            }
            else
            {
                thenBlock.Add(ParseStatement());
            }
        }

        return new IfStatement(condition, thenBlock, elseBlock, elifStatements);
    }



    private Statement ParseFunctionStatement()
    {
        string functionName = Expect(TokenType.Identifier).Value;
        Expect(TokenType.Delimiter, "(");
        List<string> parameters = ParseParameters();

        List<Statement> body = new List<Statement>();
        while (!Match(TokenType.Keyword, "endf"))
        {
            body.Add(ParseStatement());
        }

        return new FunctionStatement(functionName, parameters, body, new Dictionary<string, object>(variables));
    }

    private Statement ParseSuperStatement()
    {
        Expect(TokenType.Delimiter, "(");
        List<Expression> arguments = ParseArguments();
        return new SuperMethodCallStatement("__init__", arguments);
    }

    private Statement ParseInitStatement()
    {
        Expect(TokenType.Delimiter, "(");
        List<string> parameters = ParseParameters();
        List<Statement> body = ParseBlock("endinit");
        Expect(TokenType.Keyword, "endinit");
        return new InitStatement(parameters, body);
    }

    private Statement ParseReturnStatement()
    {
        Expression returnValue;
        if (Match(TokenType.Delimiter, "("))
        {
            returnValue = ParseExpression();
            Expect(TokenType.Delimiter, ")");
        }
        else
        {
            returnValue = ParseExpression();
        }
        return new ReturnStatement(returnValue);
    }

    private Statement ParseWhileStatement()
    {
        Expect(TokenType.Delimiter, "(");
        Expression condition = ParseExpression();
        Expect(TokenType.Delimiter, ")");
        Expect(TokenType.Keyword, "do");
        List<Statement> body = ParseBlock("endwhile");
        Expect(TokenType.Keyword, "endwhile");
        return new WhileStatement(condition, body);
    }

    private Statement ParseForStatement()
    {
        Expect(TokenType.Delimiter, "(");
        if (Match(TokenType.Identifier))
        {
            string variable = PreviousToken.Value;
            if (Match(TokenType.Keyword, "in"))
            {
                Expression iterableExpression = ParseExpression();
                Expect(TokenType.Delimiter, ")");
                Expect(TokenType.Keyword, "do");
                List<Statement> body = ParseBlock("endfor");
                Expect(TokenType.Keyword, "endfor");
                return new ForEachStatement(variable, iterableExpression, body);
            }
            else if (Match(TokenType.Operator, "="))
            {
                Expression startValue = ParseExpression();
                Expect(TokenType.Keyword, "to");
                Expression endValue = ParseExpression();
                Expect(TokenType.Delimiter, ")");
                Expect(TokenType.Keyword, "do");
                List<Statement> body = ParseBlock("endfor");
                Expect(TokenType.Keyword, "endfor");
                return new ForStatement(variable, startValue, endValue, body);
            }
        }
        else if (Match(TokenType.Delimiter, "("))
        {
            string firstVariable = Expect(TokenType.Identifier).Value;
            Expect(TokenType.Delimiter, ",");
            string secondVariable = Expect(TokenType.Identifier).Value;
            Expect(TokenType.Delimiter, ")");
            Expect(TokenType.Keyword, "in");
            Expression iterableExpression = ParseExpression();
            Expect(TokenType.Delimiter, ")");
            Expect(TokenType.Keyword, "do");
            List<Statement> body = ParseBlock("endfor");
            Expect(TokenType.Keyword, "endfor");
            return new ForEachKVStatement(firstVariable, secondVariable, iterableExpression, body);
        }
        throw new Exception("Invalid for statement syntax");
    }

    private Statement ParseClassStatement()
    {
        string className = Expect(TokenType.Identifier).Value;
        Expression baseClassExpression = null;
        if (Match(TokenType.Keyword, "extends"))
        {
            baseClassExpression = ParseExpression();
        }
        List<Statement> body = ParseBlock("endclass");
        Expect(TokenType.Keyword, "endclass");
        return new ClassStatement(className, baseClassExpression, body);
    }

    private Statement ParseShareStatement()
    {
        Expect(TokenType.Delimiter, "(");
        List<string> importedItems = new List<string>();
        while (!Match(TokenType.Delimiter, ")"))
        {
            importedItems.Add(Expect(TokenType.Identifier).Value);
            Match(TokenType.Delimiter, ",");
        }
        return new ShareStatement(importedItems, SharedVariables);
    }

    private Statement ParseImportStatement()
    {
        string fileName = Expect(TokenType.String).Value;
        Expect(TokenType.Keyword, "use");

        if (Match(TokenType.Operator, "*"))
        {
            return new ImportStatement(fileName, new List<string>(), true);
        }
        else
        {
            Expect(TokenType.Delimiter, "{");
            List<string> importedItems = new List<string>();
            while (!Match(TokenType.Delimiter, "}"))
            {
                importedItems.Add(Expect(TokenType.Identifier).Value);
                Match(TokenType.Delimiter, ",");
            }
            return new ImportStatement(fileName, importedItems, false);
        }
    }

    private Statement ParseLoadStatement()
    {
        string fileName = Expect(TokenType.String).Value;
        Expect(TokenType.Keyword, "as");
        string alias = Expect(TokenType.Identifier).Value;
        return new LoadStatement(fileName, alias);
    }

    private Statement ParseAssignmentOrFunctionCall()
    {
        string identifier = PreviousToken.Value;
        Expression target = new VariableExpression(identifier);

        while (Match(TokenType.Delimiter, "[") || Match(TokenType.Delimiter, "."))
        {
            if (PreviousToken.Value == "[")
            {
                Expression key = ParseExpression();
                Expect(TokenType.Delimiter, "]");
                target = new IndexAccessExpression(target, key);
            }
            else if (PreviousToken.Value == ".")
            {
                string propertyName = Expect(TokenType.Identifier).Value;

                if (Match(TokenType.Delimiter, "("))
                {
                    List<Expression> arguments = ParseArguments();
                    return new MethodCallStatement(identifier, propertyName, arguments);
                }
                else
                {
                    target = new IndexAccessExpression(target, new StringExpression(propertyName));
                }
            }
        }

        if (Match(TokenType.Operator, "="))
        {
            Expression value = ParseExpression();
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
        else if (Match(TokenType.Delimiter, "("))
        {
            List<Expression> arguments = ParseArguments();
            return new FunctionCallStatement(identifier, arguments);
        }

        throw new Exception($"Unexpected token after identifier '{identifier}': '{CurrentToken.Value}'");
    }


    private List<Statement> ParseBlock(params string[] terminators)
    {
        List<Statement> block = new List<Statement>();
        while (currentTokenIndex < tokens.Count && !IsTerminator(terminators))
        {
            block.Add(ParseStatement());
        }
        return block;
    }

    private bool IsTerminator(string[] terminators)
    {
        return CurrentToken.Type == TokenType.Keyword && terminators.Contains(CurrentToken.Value);
    }

    private Expression ParseExpression()
    {
        return ParseLogicalOr();
    }

    private Expression ParseLogicalOr()
    {
        Expression left = ParseLogicalAnd();
        while (Match(TokenType.Operator, "||"))
        {
            string op = PreviousToken.Value;
            Expression right = ParseLogicalAnd();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseLogicalAnd()
    {
        Expression left = ParseIn();
        while (Match(TokenType.Operator, "&&"))
        {
            string op = PreviousToken.Value;
            Expression right = ParseIn();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseIn()
    {
        Expression left = ParseEquality();
        while (Match(TokenType.Keyword, "in"))
        {
            string op = PreviousToken.Value;
            Expression right = ParseEquality();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseEquality()
    {
        Expression left = ParseRelational();
        while (Match(TokenType.Operator, "==") || Match(TokenType.Operator, "!="))
        {
            string op = PreviousToken.Value;
            Expression right = ParseRelational();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseRelational()
    {
        Expression left = ParseArithmetic();
        while (Match(TokenType.Operator, ">") || Match(TokenType.Operator, "<") ||
               Match(TokenType.Operator, ">=") || Match(TokenType.Operator, "<="))
        {
            string op = PreviousToken.Value;
            Expression right = ParseArithmetic();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseArithmetic()
    {
        Expression left = ParseTerm();
        while (Match(TokenType.Operator, "+") || Match(TokenType.Operator, "-"))
        {
            string op = PreviousToken.Value;
            Expression right = ParseTerm();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseTerm()
    {
        Expression left = ParsePower();
        while (Match(TokenType.Operator, "*") || Match(TokenType.Operator, "/") ||
               Match(TokenType.Operator, "%") || Match(TokenType.Operator, "//"))
        {
            string op = PreviousToken.Value;
            Expression right = ParsePower();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParsePower()
    {
        Expression left = ParseFactor();
        while (Match(TokenType.Operator, "**"))
        {
            string op = PreviousToken.Value;
            Expression right = ParseFactor();
            left = new BinaryExpression(left, op, right);
        }
        return left;
    }

    private Expression ParseFactor()
    {
        Expression expr = null;

        if (Match(TokenType.Operator, "-"))
        {
            Expression factor = ParseFactor();
            return new UnaryExpression("-", factor);
        }

        if (Match(TokenType.Number))
        {
            if (int.TryParse(PreviousToken.Value, out int intValue))
            {
                expr = new ConstantExpression(intValue);
            }
            else if (double.TryParse(PreviousToken.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleValue))
            {
                expr = new ConstantExpression(doubleValue);
            }
        }
        else if (Match(TokenType.Keyword, "True") || Match(TokenType.Keyword, "False"))
        {
            expr = new BooleanExpression(PreviousToken.Value == "True");
        }
        else if (Match(TokenType.String))
        {
            expr = new StringExpression(PreviousToken.Value);
        }
        else if (Match(TokenType.Keyword, "null"))
        {
            expr = new NullExpression();
        }
        else if (Match(TokenType.Delimiter, "["))
        {
            expr = ParseListExpression();
        }
        else if (Match(TokenType.Keyword, "new"))
        {
            expr = ParseNewExpression();
        }
        else if (Match(TokenType.Delimiter, "{"))
        {
            expr = ParseObjectExpression();
        }
        else if (Match(TokenType.Identifier))
        {
            expr = ParseVariableOrFunctionCall();
        }
        else if (Match(TokenType.Delimiter, "("))
        {
            expr = ParseExpression();
            Expect(TokenType.Delimiter, ")");
        }

        while (Match(TokenType.Delimiter, "."))
        {
            string propertyName = Expect(TokenType.Identifier).Value;

            if (Match(TokenType.Delimiter, "("))
            {
                List<Expression> arguments = ParseArguments();
                expr = new MethodCallExpression(expr, propertyName, arguments);
            }
            else
            {
                expr = new PropertyAccessExpression(expr, propertyName);
            }
        }

        if (expr == null)
        {
            throw new Exception($"Unexpected token: {CurrentToken.Value}");
        }

        return expr;
    }


    private Expression ParseListExpression()
    {
        var elements = new List<Expression>();
        while (!Match(TokenType.Delimiter, "]"))
        {
            elements.Add(ParseExpression());
            if (!Match(TokenType.Delimiter, ","))
            {
                break;
            }
        }
        Expect(TokenType.Delimiter, "]");
        return new ListExpression(elements);
    }

    private Expression ParseNewExpression()
    {
        var classPath = new List<string>();
        classPath.Add(Expect(TokenType.Identifier).Value);

        while (Match(TokenType.Delimiter, "."))
        {
            classPath.Add(Expect(TokenType.Identifier).Value);
        }

        Expect(TokenType.Delimiter, "(");
        var arguments = ParseArguments();
        return new NewExpression(classPath, arguments);
    }

    private Expression ParseObjectExpression()
    {
        var properties = new Dictionary<string, Expression>();
        while (!Match(TokenType.Delimiter, "}"))
        {
            string key;
            if (Match(TokenType.Identifier))
            {
                key = PreviousToken.Value;
            }
            else if (Match(TokenType.String))
            {
                key = PreviousToken.Value;
            }
            else
            {
                throw new Exception($"Expected Identifier or String for object key, but found {CurrentToken.Type} at line {CurrentToken.Line}, column {CurrentToken.Column}");
            }

            Expect(TokenType.Delimiter, ":");
            Expression value = ParseExpression();
            properties.Add(key, value);
            if (!Match(TokenType.Delimiter, ","))
            {
                break;
            }
        }
        Expect(TokenType.Delimiter, "}");
        return new ObjectExpression(properties);
    }

    private Expression ParseVariableOrFunctionCall()
    {
        string identifier = PreviousToken.Value;
        Expression expr = new VariableExpression(identifier);

        while (true)
        {
            if (Match(TokenType.Delimiter, "("))
            {
                List<Expression> arguments = ParseArguments();
                expr = new FunctionCallExpression(identifier, arguments);
            }
            else if (Match(TokenType.Delimiter, "["))
            {
                Expression key = ParseExpression();
                Expect(TokenType.Delimiter, "]");
                expr = new IndexAccessExpression(expr, key);
            }
            else if (Match(TokenType.Delimiter, "."))
            {
                string memberName = Expect(TokenType.Identifier).Value;
                if (Match(TokenType.Delimiter, "("))
                {
                    var arguments = ParseArguments();
                    expr = new MethodCallExpression(expr, memberName, arguments);
                }
                else
                {
                    expr = new PropertyAccessExpression(expr, memberName);
                }
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private List<string> ParseParameters()
    {
        List<string> parameters = new List<string>();
        while (!Match(TokenType.Delimiter, ")"))
        {
            parameters.Add(Expect(TokenType.Identifier).Value);
            if (!Match(TokenType.Delimiter, ","))
            {
                Expect(TokenType.Delimiter, ")");
                break;
            }
        }
        return parameters;
    }

    private List<Expression> ParseArguments()
    {
        List<Expression> arguments = new List<Expression>();
        while (!Match(TokenType.Delimiter, ")"))
        {
            arguments.Add(ParseExpression());
            if (!Match(TokenType.Delimiter, ","))
            {
                Expect(TokenType.Delimiter, ")");
                break;
            }
        }
        return arguments;
    }

    private Token CurrentToken => tokens[currentTokenIndex];
    private Token PreviousToken => tokens[currentTokenIndex - 1];

    private bool Match(TokenType type)
    {
        if (CurrentToken.Type == type)
        {
            currentTokenIndex++;
            return true;
        }
        return false;
    }

    private bool Match(TokenType type, string value)
    {
        if (CurrentToken.Type == type && CurrentToken.Value == value)
        {
            currentTokenIndex++;
            return true;
        }
        return false;
    }

    private Token Expect(TokenType type)
    {
        if (CurrentToken.Type != type)
        {
            throw new Exception($"Expected {type}, but found {CurrentToken.Type} at line {CurrentToken.Line}");
        }
        return tokens[currentTokenIndex++];
    }
    
    private Token Expect(TokenType type, string value)
    {
        if (CurrentToken.Type != type || CurrentToken.Value != value)
        {
            throw new Exception($"Expected {type} with value '{value}', but found {CurrentToken.Type} with value '{CurrentToken.Value}'");
        }
        return tokens[currentTokenIndex++];
    }
}