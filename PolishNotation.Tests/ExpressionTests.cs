using Xunit;

namespace PolishNotation.Tests;

public class ExpressionTests
{
    [Fact]
    public void Test1()
    {
        // Arrange
        string stringExpression = "5";

        // Act
        double result = Expression.Calculate(stringExpression);

        // Assert
        Assert.Equal(5, result);
    }
}
