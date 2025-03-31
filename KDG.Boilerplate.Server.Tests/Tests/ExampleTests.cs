using System;
using Xunit;
using KDG.Boilerplate.Services;

namespace KDG.Boilerplate.Server.Tests.Validation
{

    public class ExampleTests
    {
        [Fact]
        public void ExampleService_Add_ReturnsSum()
        {
            // Arrange
            var exampleService = new ExampleService();
            // Act
            var result = exampleService.Add(1, 2);
            // Assert
            Assert.Equal(3, result);
        }

        [Fact]
        public void ExampleService_Subtract_ReturnsDifference()
        {
            // Arrange
            var exampleService = new ExampleService();
            // Act
            var result = exampleService.Subtract(2, 1);
            // Assert
            Assert.Equal(1, result);
        }
    }
}
