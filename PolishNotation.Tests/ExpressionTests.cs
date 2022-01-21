using System;
using Xunit;

namespace PolishNotation.Tests;

public class ExpressionTests
{
    [Theory]
    [InlineData("7,42 +", 49)]
    [InlineData("7,42 -", -35)]
    [InlineData("3,6 *", 18)]
    [InlineData("6,3 *", 18)]
    [InlineData("10,5 /", 2)]
    [InlineData("5,10 /", 0.5)]
    [InlineData("5", 5)]
    [InlineData("pi,3 +", Math.PI + 3)]
    [InlineData("pi,1.5 * sin", -1)]
    [InlineData("pi, cos", -1)]
    [InlineData("230(220,0.5 *) -", 120)]
    [InlineData("16,2 log", 4)]
    [InlineData("e 7^ ln", 7)]
    public void ToDelegate_FromString_ReturnsExpected(string stringExpression, double expected)
    {
        Func<double, double> @delegate = Expression.ToDelegate(stringExpression);
        double result = @delegate(double.NaN);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("7,42 +", 49)]
    [InlineData("5", 5)]
    [InlineData("pi,3 +", Math.PI + 3)]
    public void Calculate_FromString_ReturnsExpected(string stringExpression, double expected)
    {
        double result = Expression.Calculate(stringExpression);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("7,x +", 42, 49)]
    [InlineData("X", 7, 7)]
    [InlineData("x", 5, 5)]
    [InlineData("pi,x +", 6, Math.PI + 6)]
    public void Calculate_WithX_ReturnsExpected(string stringExpression, double xValue, double expected)
    {
        double result = Expression.Calculate(stringExpression, xValue);

        Assert.Equal(expected, result);
    }
}
