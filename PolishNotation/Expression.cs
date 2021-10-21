using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolishNotation;


public class Expression : IEnumerable<ElementaryUnit>
{
    private static readonly Dictionary<char, Func<double, double, double>> binaryOperators = new()
    {
        { '+', (a, b) => a + b },
        { '-', (a, b) => a - b },
        { '*', (a, b) => a * b },
        { '/', (a, b) => a / b },
        { '^', (a, b) => Math.Pow(a, b) }
    };

    private static readonly Dictionary<string, Func<double, double>> unaryFunctions = new()
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
        { "arctg", (number) => 1 / Math.Atan(number) },
        { "lg", Math.Log10 },
        { "ln", (number) => Math.Log(number) },
        { "log10", Math.Log10 }
    };

    private static Dictionary<string, double> Constants { get; } = new()
    {
        { "pi", Math.PI },
        { "e", Math.E }
    };

    private static readonly Dictionary<string, Func<double, double, double>> binaryFunction = new()
    {
        { "log", (a, b) => Math.Log(a, b) },
        { "test", (a, b) => a + b + 10 }
    };

    private static Expression ToExpresion(string stringRepresentation)
    {
        stringRepresentation = new string(stringRepresentation.Where(c => c != ' ').ToArray());

        StringBuilder buf = new(Convert.ToString(stringRepresentation[0]));

        for (int i = 1; i < stringRepresentation.Length; i++)
        {
            if (stringRepresentation[i] == '-' && stringRepresentation[i - 1] == '(')
            {
                buf.Append('0');
            }
            buf.Append(stringRepresentation[i]);
        }

        stringRepresentation = buf.ToString();

        if (stringRepresentation[0] == '-')
        {
            stringRepresentation = "0" + stringRepresentation;
        }

        List<ElementaryUnit>? result = new();

        for (int i = 0; i < stringRepresentation.Length; i++)
        {
            char c = stringRepresentation[i];

            if (c == 'x' || c == 'X')
            {
                result.Add(new ElementaryUnit(ElementaryUnitType.Variable, Convert.ToString(c)));
                continue;
            }

            if (binaryOperators.Any(operation => operation.Key == c))
            {
                result.Add(new ElementaryUnit(ElementaryUnitType.BinaryOperation, Convert.ToString(c)));
                continue;
            }

            if (c == '(' || c == ')')
            {
                result.Add(new ElementaryUnit(ElementaryUnitType.Brackets, Convert.ToString(c)));
                continue;
            }

            if (char.IsLetter(c))
            {
                string? buffer = string.Empty;
                int j = i;
                for (; j < stringRepresentation.Length && char.IsLetter(stringRepresentation[j]); j++)
                {
                    buffer += stringRepresentation[j];
                }
                i = j - 1;

                if (unaryFunctions.ContainsKey(buffer))
                    result.Add(new ElementaryUnit(ElementaryUnitType.UnaryFunction, buffer));
                if (binaryFunction.ContainsKey(buffer))
                    result.Add(new ElementaryUnit(ElementaryUnitType.BinaryFunction, buffer));
                if (Constants.ContainsKey(buffer))
                    result.Add(new ElementaryUnit(ElementaryUnitType.Constant, buffer));
                continue;
            }

            if (char.IsDigit(c))
            {
                string? buffer = string.Empty;
                int j = i;
                for (; j < stringRepresentation.Length && (char.IsDigit(stringRepresentation[j]) || stringRepresentation[j] == '.'); j++)
                {
                    buffer += stringRepresentation[j];
                }
                i = j - 1;
                result.Add(new ElementaryUnit(ElementaryUnitType.Digit, buffer));
            }

        }


        return new Expression(result);
    }

    private static Expression ToPolishNotation(Expression expression)
    {
        List<ElementaryUnit>? result = new();
        Stack<ElementaryUnit>? buffer = new();

        string? firstLO = "+-";
        string? secondLO = "*/";

        foreach (ElementaryUnit? el in expression._Expression)
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
        {
            result.Add(buffer.Pop());
        }

        return new Expression(result);
    }

    private static Func<double, double> ToFunc(Expression expression)
    {
        return (x) =>
        {
            return Calculate(expression, x);
        };
    }

    private static double Calculate(Expression expression, double x)
    {
        Stack<double>? stack = new();

        foreach (ElementaryUnit? el in expression._Expression)
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
                Func<double, double>? f = unaryFunctions[el.Value];

                double arg = stack.Pop();

                stack.Push(f(arg));
            }

            if (el.Type == ElementaryUnitType.BinaryFunction)
            {
                double a = stack.Pop();

                double b = stack.Pop();

                stack.Push(binaryFunction[el.Value](b, a));
            }

            if (el.Type == ElementaryUnitType.BinaryOperation)
            {
                double a = stack.Pop();
                double b = stack.Pop();
                stack.Push(binaryOperators[el.Value[0]](a, b));
            }
        }


        return stack.Pop();
    }

    internal IEnumerator<ElementaryUnit> GetEnumerator()
    {
        return _Expression.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _Expression.GetEnumerator();
    }

    internal Expression(ICollection<ElementaryUnit> expression)
    {
        _Expression = expression;
    }

    private readonly ICollection<ElementaryUnit> _Expression;

    public override string ToString()
    {
        string res = string.Empty;
        foreach (ElementaryUnit? el in _Expression)
        {
            res += el.Value + " ";
        }
        return res;
    }

    public static Func<double, double> ToDelegate(string expression)
    {
        Expression exp = ToExpresion(expression);
        Expression inversePolishNotation = ToPolishNotation(exp);
        return ToFunc(inversePolishNotation);
    }

    public static double Calculate(string expression, double x = 0)
    {
        Expression exp = ToExpresion(expression);
        //exp.Select(e => e.Type.ToString()).ToList().ForEach(Console.WriteLine);
        Expression inversePolishNotation = ToPolishNotation(exp);
        return Calculate(inversePolishNotation, x);
    }

    IEnumerator<ElementaryUnit> IEnumerable<ElementaryUnit>.GetEnumerator()
    {
        return _Expression.GetEnumerator();
    }
}
