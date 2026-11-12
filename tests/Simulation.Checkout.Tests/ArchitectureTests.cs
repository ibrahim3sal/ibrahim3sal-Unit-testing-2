
using FluentAssertions;
using NetArchTest.Rules;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Infrastructure;

namespace Simulation.Checkout.Tests;

public class ArchitectureTests
{
    [Fact]
    public void Domain_ShouldNotDependOnApplicationOrInfrastructure()
    {
        // Arrange
        var result = Types.InAssembly(typeof(Order).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Application")
            .And()
            .NotHaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructure()
    {
        // Arrange
        var result = Types.InAssembly(typeof(CheckoutService).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Simulation.Checkout.Infrastructure")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void AllInterfaces_ShouldStartWithI()
    {
        // Arrange
        var assemblies = new[]
        {
            typeof(Order).Assembly,
            typeof(CheckoutService).Assembly,
            typeof(InMemoryOrderRepository).Assembly
        };

        // Act
        var results = assemblies.Select(assembly =>
            Types.InAssembly(assembly)
                .That()
                .AreInterfaces()
                .Should()
                .HaveNameStartingWith("I")
                .GetResult());

        // Assert
        results.Should().OnlyContain(result => result.IsSuccessful);
    }

    [Fact]
    public void OrderRepositoryImplementations_ShouldEndWithRepository()
    {
        // Arrange
        var result = Types.InAssembly(typeof(InMemoryOrderRepository).Assembly)
            .That()
            .ImplementInterface(typeof(IOrderRepository))
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Act & Assert
        result.IsSuccessful.Should().BeTrue();
    }
}
