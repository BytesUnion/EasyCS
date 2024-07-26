class BinaryExpression : Expression
{
    private Expression left;
    private string op;
    private Expression right;

    public BinaryExpression(Expression left, string op, Expression right)
    {
        this.left = left;
        this.op = op;
        this.right = right;
    }

    private object EvaluateLogical(object leftValue, object rightValue)
    {
        if (op == "||")
        {
            return Convert.ToBoolean(leftValue) || Convert.ToBoolean(rightValue);
        }
        else if (op == "&&")
        {
            return Convert.ToBoolean(leftValue) && Convert.ToBoolean(rightValue);
        }
        throw new Exception($"'{op}' isn't a valid choice for logical operations.");
    }

    public override object Evaluate(Dictionary<string, object> variables, Dictionary<string, FunctionStatement> functions)
    {
        var leftValue = left.Evaluate(variables, functions);

        if (op == "||" && Convert.ToBoolean(leftValue))
        {
            return true;
        }
        else if (op == "&&" && !Convert.ToBoolean(leftValue))
        {
            return false;
        }

        var rightValue = right.Evaluate(variables, functions);

        if (op == "||" || op == "&&")
        {
            return EvaluateLogical(leftValue, rightValue);
        }

        ESConvert converter = new ESConvert();

        switch (op)
        {
            case "==":
                if (leftValue is string leftStr && rightValue is string rightStr)
                    return leftStr == rightStr;
                return leftValue.Equals(rightValue);
            case "!=":
                if (leftValue is string leftStr2 && rightValue is string rightStr2)
                    return leftStr2 != rightStr2;
                return !leftValue.Equals(rightValue);
            case ">":
                return Convert.ToDouble(leftValue) > Convert.ToDouble(rightValue);
            case "<":
                return Convert.ToDouble(leftValue) < Convert.ToDouble(rightValue);
            case ">=":
                return Convert.ToDouble(leftValue) >= Convert.ToDouble(rightValue);
            case "<=":
                return Convert.ToDouble(leftValue) <= Convert.ToDouble(rightValue);
            case "+":
                if (leftValue is string leftStrVal && rightValue is string rightStrVal)
                    return leftStrVal + rightStrVal;
                else if (leftValue is string && rightValue is string == false)
                    throw new Exception("Can't concatenate strings with other types of data.");
                else if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '+' for operands of type '{converter.Convert(leftValue)}' & '{converter.Convert(rightValue)}'");
                else if (leftValue is int leftIntVal && rightValue is int rightIntVal)
                    return leftIntVal + rightIntVal;
                return Convert.ToDouble(leftValue) + Convert.ToDouble(rightValue);
            case "-":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '-' for operands of type '{converter.Convert(leftValue)}' & '{converter.Convert(rightValue)}'");
                if (leftValue is int leftIntVal2 && rightValue is int rightIntVal2)
                    return leftIntVal2 - rightIntVal2;
                return Convert.ToDouble(leftValue) - Convert.ToDouble(rightValue);
            case "*":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '*' for operands of type '{converter.Convert(leftValue)}' & '{converter.Convert(rightValue)}'");
                if (leftValue is int leftIntVal3 && rightValue is int rightIntVal3)
                    return leftIntVal3 * rightIntVal3;
                return Convert.ToDouble(leftValue) * Convert.ToDouble(rightValue);
            case "/":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '/' for operands of type '{converter.Convert(leftValue)} ' & ' {converter.Convert(rightValue)}'");
                if (Convert.ToInt32(rightValue) == 0) throw new Exception("Uh-oh! You tried to divide by zero. That's not possible.");
                return Convert.ToDouble(leftValue) / Convert.ToDouble(rightValue);
            case "//":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '//' for operands of type '{converter.Convert(leftValue)} ' & ' {converter.Convert(rightValue)}'");
                if (Convert.ToInt32(rightValue) == 0) throw new Exception("Uh-oh! You tried to divide by zero. That's not possible.");
                if (leftValue is int leftIntVal4 && rightValue is int rightIntVal4)
                    return leftIntVal4 / rightIntVal4;
                return Convert.ToInt32(Math.Floor(Convert.ToDouble(leftValue) / Convert.ToDouble(rightValue)));
            case "%":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '%' for operands of type '{converter.Convert(leftValue)} ' & ' {converter.Convert(rightValue)}'");
                if (Convert.ToInt32(rightValue) == 0) throw new Exception("Uh-oh! You tried to divide by zero. That's not possible.");
                return Convert.ToDouble(leftValue) % Convert.ToDouble(rightValue);
            case "**":
                if ((leftValue is int == true && rightValue is double == true) == false && (leftValue is double == true && rightValue is int == true) == false && (leftValue is int == true && rightValue is int == true) == false && (leftValue is double == true && rightValue is double == true) == false)
                    throw new Exception($"Can't use '**' for operands of type '{converter.Convert(leftValue)}' & '{converter.Convert(rightValue)}'");
                return Math.Pow(Convert.ToDouble(leftValue), Convert.ToDouble(rightValue));
            default:
                throw new Exception($"Unknown operator '{op}'");
        }
    }
}
