using System;
using Xunit;
using PolishNotation;

namespace PolishNotation.Tests
{
    public class ExpressionTests
    {
        [Fact]
        public void Test1()
        {
            // Arrange
            var stringExpression = "5";

            // Act
            var result = Expression.Calculate(stringExpression);

            // Assert
            Assert.Equal(5, result);
        }
    }
}
