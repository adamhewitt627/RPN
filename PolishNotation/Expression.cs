using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolishNotation;

public class Expression : IEnumerable<ElementaryUnit>
{
    private readonly List<ElementaryUnit> _Expression;

    private static HashSet<char> BinaryOperators { get; } = new() { '+', '-', '*', '/', '^' };

    private static Dictionary<string, Func<double, double>> UnaryFunctions { get; } = new()
    {
        { "sin", Math.Sin },
        { "cos", Math.Cos },
        { "tg", Math.Tan },
        { "ctg", (number) => Math.Cos(number) / Math.Sin(number) },
        { "sign", (number) => Math.Sign(number) },
        { "sqrt", Math.Sqrt },
        { "abs", Math.Abs },
        { "acos", Math.Acos },
        { "asin", Math.Asin },
        { "atan", Math.Atan },
        { "actg", (number) => 1 / Math.Atan(number) },
        { "lg", Math.Log10 },
        { "ln", (number) => Math.Log(number) }
    };

    private static Dictionary<string, double> Constants { get; } = new()
    {
        { "pi", Math.PI },
        { "e", Math.E }
    };

    private static Dictionary<string, Func<double, double, double>> BinaryFunctions { get; } = new()
    {
        { "log", (a, b) => Math.Log(a, b) }
    };

    private Expression(List<ElementaryUnit> expression)
    {
        _Expression = expression;
    }


    private static Expression ToExpression(string expression)
    {
        expression = Fixup(expression);

        List<ElementaryUnit> result = new();

        for (var i = 0; i < expression.Length; i++)
        {
            char c = expression[i];

            switch (c)
            {
                case 'x' or 'X':
                    result.Add(new ElementaryUnit(ElementaryUnitType.Variable, Convert.ToString(c)));
                    break;

                case '(' or ')':
                    result.Add(new ElementaryUnit(ElementaryUnitType.Brackets, Convert.ToString(c)));
                    break;

                case { } when BinaryOperators.Contains(c):
                    result.Add(new ElementaryUnit(ElementaryUnitType.BinaryOperation, Convert.ToString(c)));
                    break;

                case { } when char.IsLetter(c):
                    {
                        string buffer = Collect(expression, i, char.IsLetter);
                        i += buffer.Length - 1;

                        if (UnaryFunctions.ContainsKey(buffer))
                            result.Add(new ElementaryUnit(ElementaryUnitType.UnaryFunction, buffer));
                        else if (BinaryFunctions.ContainsKey(buffer))
                            result.Add(new ElementaryUnit(ElementaryUnitType.BinaryFunction, buffer));
                        else if (Constants.ContainsKey(buffer))
                            result.Add(new ElementaryUnit(ElementaryUnitType.Constant, buffer));
                    }
                    break;

                case { } when char.IsDigit(c):
                    {
                        string buffer = Collect(expression, i, x => char.IsDigit(x) || x == '.');
                        i += buffer.Length - 1;
                        result.Add(new ElementaryUnit(ElementaryUnitType.Digit, buffer));
                    }
                    break;
            }
        }

        return new Expression(result);


        static string Collect(string expression, int from, Func<char, bool> takeWhile)
        {
            int end = from;
            for (; end < expression.Length && takeWhile(expression[end]); end++) { }

            return expression.Substring(from, end - from);
        }

        static string Fixup(string expression)
        {
            StringBuilder builder = new();
            char[] stringRepresentation = expression.Where(c => c != ' ').ToArray();

            if (stringRepresentation[0] == '-') builder.Append('-');
            builder.Append(stringRepresentation[0]);

            for (int i = 1; i < stringRepresentation.Length; i++)
            {
                if (stringRepresentation[i] is '-' && stringRepresentation[i - 1] is '(')
                {
                    builder.Append('0');
                }

                builder.Append(stringRepresentation[i]);
            }

            return builder.ToString();
        }

    }

    private static Expression ToPolishNotation(Expression expression)
    {
        List<ElementaryUnit> result = new();
        Stack<ElementaryUnit> buffer = new();

        var firstLO = "+-";
        var secondLO = "*/";

        foreach (var el in expression)
        {
            if (el.Type == ElementaryUnitType.Digit || el.Type == ElementaryUnitType.Variable || el.Type == ElementaryUnitType.Constant)
            {
                result.Add(el);
                continue;
            }
            if (el.Type == ElementaryUnitType.BinaryFunction || el.Type == ElementaryUnitType.UnaryFunction)
            {
                buffer.Push(el);
                continue;
            }

            if (el.Type == ElementaryUnitType.Brackets)
            {
                if (el.Value == ")")
                {
                    while (buffer.Peek().Value != "(")
                    {
                        result.Add(buffer.Pop());
                    }
                    if (el.Type == ElementaryUnitType.BinaryFunction || el.Type == ElementaryUnitType.UnaryFunction)
                    {
                        result.Add(buffer.Pop());
                    }
                    buffer.Pop();
                    continue;
                }

                buffer.Push(el);
            }

            if (el.Type == ElementaryUnitType.BinaryOperation)
            {
                if (el.Value == "^")
                {
                    while (buffer.Count != 0 && (buffer.Peek().Value == "^" || el.Type == ElementaryUnitType.BinaryFunction || el.Type == ElementaryUnitType.UnaryFunction))
                    {
                        result.Add(buffer.Pop());
                        if (buffer.Count == 0) break;
                    }

                    buffer.Push(el);
                    continue;
                }
                if (firstLO.Contains(el.Value))
                {
                    if (buffer.Count != 0)
                        while ((firstLO + secondLO).Contains(buffer.Peek().Value) || buffer.Peek().Value == "^" || buffer.Peek().Type == ElementaryUnitType.BinaryFunction || buffer.Peek().Type == ElementaryUnitType.UnaryFunction)
                        {
                            result.Add(buffer.Pop());
                            if (buffer.Count == 0) break;
                        }
                    buffer.Push(el);
                    continue;
                }
                if (secondLO.Contains(el.Value))
                {
                    if (buffer.Count != 0)
                        while ((buffer.Count != 0 && secondLO.Contains(buffer.Peek().Value)) || (buffer.Peek().Value == "^" && buffer.Count != 0) || buffer.Peek().Type == ElementaryUnitType.BinaryFunction || buffer.Peek().Type == ElementaryUnitType.UnaryFunction)
                        {
                            result.Add(buffer.Pop());
                            if (buffer.Count == 0) break;
                        }

                    buffer.Push(el);
                }
            }
        }

        while (buffer.Count != 0)
            result.Add(buffer.Pop());

        return new Expression(result);
    }

    private static double Calculate(Expression expression, double x)
    {
        Stack<double> stack = new();

        foreach (var el in expression)
        {
            if (el.Type == ElementaryUnitType.Digit)
            {
                stack.Push(Convert.ToDouble(el.Value));
                continue;
            }

            if (el.Type == ElementaryUnitType.Constant)
            {
                stack.Push(Constants[el.Value]);
                continue;
            }

            if (el.Type == ElementaryUnitType.Variable)
            {
                stack.Push(x);
                continue;
            }

            if (el.Type == ElementaryUnitType.UnaryFunction)
            {
                if (el.Value == "log")
                {
                    var a = stack.Pop();

                    var b = stack.Pop();

                    stack.Push(Math.Log(b, a));

                    continue;
                }
                var f = UnaryFunctions[el.Value];

                var arg = stack.Pop();

                stack.Push(f(arg));
            }

            if (el.Type == ElementaryUnitType.BinaryFunction)
            {
                var a = stack.Pop();

                var b = stack.Pop();

                stack.Push(BinaryFunctions[el.Value](b, a));
            }

            if (el.Type == ElementaryUnitType.BinaryOperation)
            {
                var a = stack.Pop();
                var b = stack.Pop();
                switch (el.Value)
                {
                    case "+": stack.Push(a + b); break;
                    case "-": stack.Push(b - a); break;
                    case "/": stack.Push(b / a); break;
                    case "*": stack.Push(a * b); break;
                    case "^": stack.Push(Math.Pow(b, a)); break;
                }
            }
        }


        return stack.Pop();
    }

    public override string ToString()
    {
        return string.Join(" ", _Expression.Select(u => u.Value));
    }

    public static Func<double, double> ToDelegate(string expression)
    {
        var exp = ToExpression(expression);
        var inversePolishNotation = ToPolishNotation(exp);

        return x => Calculate(exp, x);
    }

    public static double Calculate(string expression, double x = 0)
    {
        var exp = ToExpression(expression);
        var inversePolishNotation = ToPolishNotation(exp);
        return Calculate(inversePolishNotation, x);
    }

    IEnumerator<ElementaryUnit> IEnumerable<ElementaryUnit>.GetEnumerator()
    {
        return _Expression.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _Expression.GetEnumerator();
    }

}
