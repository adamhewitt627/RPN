namespace PolishNotation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Enums;

    public class Expression : IEnumerable<ElementaryUnit>
    {
        private static readonly IDictionary<char, Func<double, double, double>> binaryOperators = new Dictionary<char, Func<double, double, double>>
        {
            {'+', (a,b) => a + b },
            {'-', (a,b) => a - b },
            {'*', (a,b) => a * b },
            {'/', (a,b) => a / b },
            {'^', (a,b) => Math.Pow(a,b) }
        };

        private static readonly IDictionary<string, Func<double, double>> unaryFunctions = new Dictionary<string, Func<double, double>>
        {
            {"sin", Math.Sin },
            {"cos", Math.Cos },
            {"tg", Math.Tan },
            {"ctg", (number) => Math.Cos(number)/Math.Sin(number) },
            {"sign", (number) => Math.Sign(number) },
            {"sqrt", Math.Sqrt },
            {"abs",Math.Abs },
            {"acos",Math.Acos },
            {"asin",Math.Asin },
            {"atan", Math.Atan},
            {"arctg", (number) => 1/Math.Atan(number) },
            {"lg",Math.Log10 },
            {"ln", (number) => Math.Log(number) },
            {"log10", Math.Log10}
        };

        public static IDictionary<string, double> constans = new Dictionary<string, double>
        {
            {"pi",Math.PI },
            {"e",Math.E }
        };

        private static readonly IDictionary<string, Func<double, double, double>> binaryFunction = new Dictionary<string, Func<double, double, double>>
        {
            {"log", (a,b) => Math.Log(a,b) },
            {"test", (a,b) => a + b + 10 }
        };

        private static Expression ToExpresion(string stringRepresentation)
        {
            stringRepresentation = new string(stringRepresentation.Where(c => c != ' ').ToArray());

            var buf = new StringBuilder(Convert.ToString(stringRepresentation[0]));

            for (int i = 1; i < stringRepresentation.Length; i++)
            {
                if (stringRepresentation[i] == '-' && stringRepresentation[i - 1] == '(')
                {
                    buf.Append("0");
                }
                buf.Append(stringRepresentation[i]);
            }

            stringRepresentation = buf.ToString();

            if (stringRepresentation[0] == '-') 
            {
                stringRepresentation = "0" + stringRepresentation;
            }

            var result = new List<ElementaryUnit>();

            for (var i = 0; i < stringRepresentation.Length; i++)
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
                    var buffer = string.Empty;
                    var j = i;
                    for (; j < stringRepresentation.Length && char.IsLetter(stringRepresentation[j]); j++)
                    {
                        buffer += stringRepresentation[j];
                    }
                    i = j - 1;

                    if (unaryFunctions.Keys.Contains(buffer))
                        result.Add(new ElementaryUnit(ElementaryUnitType.UnaryFunction, buffer));
                    if (binaryFunction.Keys.Contains(buffer))
                        result.Add(new ElementaryUnit(ElementaryUnitType.BinaryFunction, buffer));
                    if (constans.Keys.Contains(buffer))
                        result.Add(new ElementaryUnit(ElementaryUnitType.Constant, buffer));
                    continue;
                }

                if (char.IsDigit(c))
                {
                    var buffer = string.Empty;
                    var j = i;
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
            var result = new List<ElementaryUnit>();
            var buffer = new Stack<ElementaryUnit>();

            var firstLO = "+-";
            var secondLO = "*/";

            foreach (var el in expression.expression)
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

        private static Func<double,double> ToFunc(Expression expression)
        {
            return (x) =>
            {
                return Calculate(expression, x);
            };
        }

        private static double Calculate(Expression expression, double x)
        {
            var stack = new Stack<double>();

            foreach (var el in expression.expression)
            {
                if (el.Type == ElementaryUnitType.Digit)
                {
                    stack.Push(Convert.ToDouble(el.Value));
                    continue;
                }

                if (el.Type == ElementaryUnitType.Constant)
                {
                    stack.Push(constans[el.Value]);
                    continue;
                }

                if (el.Type == ElementaryUnitType.Variable)
                {
                    stack.Push(x);
                    continue;
                }

                if (el.Type == ElementaryUnitType.UnaryFunction)
                {
                    var f = unaryFunctions[el.Value];

                    var arg = stack.Pop();

                    stack.Push(f(arg));
                }

                if (el.Type == ElementaryUnitType.BinaryFunction)
                {
                    var a = stack.Pop();

                    var b = stack.Pop();

                    stack.Push(binaryFunction[el.Value](b, a));
                }

                if (el.Type == ElementaryUnitType.BinaryOperation)
                {
                    var a = stack.Pop();
                    var b = stack.Pop();
                    stack.Push(binaryOperators[el.Value[0]](a,b));
                }
            }


            return stack.Pop();
        }

        internal IEnumerator<ElementaryUnit> GetEnumerator()
        {
            return expression.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return expression.GetEnumerator();
        }

        internal Expression(ICollection<ElementaryUnit> expresion)
        {
            this.expression = expresion;
        }

        internal ICollection<ElementaryUnit> expression { get; }

        public override string ToString()
        {
            var res = string.Empty;
            foreach(var el in expression)
            {
                res += el.Value + " ";
            }
            return res;
        }

        public static Func<double,double> ToDelegate(string expression)
        {
            var exp = ToExpresion(expression);
            var inversePolishNotation = ToPolishNotation(exp);
            return ToFunc(inversePolishNotation);
        }

        public static double Calculate(string expression, double x = 0)
        {
            var exp = ToExpresion(expression);
            //exp.Select(e => e.Type.ToString()).ToList().ForEach(Console.WriteLine);
            var inversePolishNotation = ToPolishNotation(exp);
            return Calculate(inversePolishNotation, x);
        }

        IEnumerator<ElementaryUnit> IEnumerable<ElementaryUnit>.GetEnumerator()
        {
            return expression.GetEnumerator();
        }
    }
}
