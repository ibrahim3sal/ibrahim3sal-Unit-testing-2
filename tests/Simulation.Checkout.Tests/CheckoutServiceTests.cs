
using FluentAssertions;
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using Simulation.Checkout.Domain.Exceptions;

namespace Simulation.Checkout.Tests;

public class CheckoutServiceTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IInventoryService> _inventoryService = new();
    private readonly Mock<INotificationService> _notificationService = new();

    private CheckoutService CreateSut()
    {
        _inventoryService
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _orderRepository
            .Setup(x => x.SaveAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _notificationService
            .Setup(x => x.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        return new CheckoutService(
            _orderRepository.Object,
            _inventoryService.Object,
            _notificationService.Object);
    }

    private static Order CreateOrder(decimal price, int quantity = 1)
    {
        return new Order
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Laptop",
                    UnitPrice = price,
                    Quantity = quantity
                }
            }
        };
    }

    // 1.1 Empty order

    [Fact]
    public async Task CheckoutAsync_WhenOrderIsEmpty_ShouldThrowEmptyOrderException()
    {
        // Arrange
        var sut = CreateSut();

        var order = new Order
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<OrderItem>()
        };

        // Act
        Func<Task> act = () => sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<EmptyOrderException>();
    }

    // 1.2 Invalid quantity

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CheckoutAsync_WhenQuantityIsInvalid_ShouldThrowInvalidQuantityException(
        int quantity)
    {
        // Arrange
        var sut = CreateSut();
        var order = CreateOrder(100m, quantity);

        // Act
        Func<Task> act = () => sut.CheckoutAsync(order);

        // Assert
        var exception = await act.Should()
            .ThrowAsync<InvalidQuantityException>();

        exception.Which.Message.Should().Contain("Laptop");
    }

    // 1.3 Negative price

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsNegative_ShouldThrowInvalidPriceException()
    {
        // Arrange
        var sut = CreateSut();
        var order = CreateOrder(-0.01m);

        // Act
        Func<Task> act = () => sut.CheckoutAsync(order);

        // Assert
        await act.Should().ThrowAsync<InvalidPriceException>();
    }

    // 1.4 Zero price

    [Fact]
    public async Task CheckoutAsync_WhenPriceIsZero_ShouldCheckoutSuccessfully()
    {
        // Arrange
        var sut = CreateSut();
        var order = CreateOrder(0m);

        // Act
        var receipt = await sut.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(0m);
        receipt.Discount.Should().Be(0m);
        receipt.Total.Should().Be(0m);
    }

    // 1.5 Discount boundaries

    [Theory]
    [InlineData(999.99, 0)]
    [InlineData(1000, 100)]
    [InlineData(1500, 100)]
    public async Task CheckoutAsync_WhenSubtotalChanges_ShouldApplyCorrectDiscount(
        decimal subtotal,
        decimal expectedDiscount)
    {
        // Arrange
        var sut = CreateSut();
        var order = CreateOrder(subtotal);

        // Act
        var receipt = await sut.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(subtotal);
        receipt.Discount.Should().Be(expectedDiscount);
    }

    // 1.6 Examples A and C
    [Fact]
    public async Task CheckoutAsync_WhenUsingExampleA_ShouldCalculateCorrectAmounts()
    {
        // Arrange
        var sut = CreateSut();
        var order = new Order
        {
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product A",
                    UnitPrice = 150m,
                    Quantity = 2
                },
                new()
                {
                    ProductId = Guid.NewGuid(),
                    ProductName = "Product B",
                    UnitPrice = 400m,
                    Quantity = 1
                }
            }
        };

        // Act
        var receipt = await sut.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(700m);
        receipt.Discount.Should().Be(0m);
        receipt.Total.Should().Be(700m);
    }

    [Fact]
    public async Task CheckoutAsync_WhenUsingExampleC_ShouldCalculateCorrectAmounts()
    {
        // Arrange
        var sut = CreateSut();
        var order = CreateOrder(500m, 3);

        // Act
        var receipt = await sut.CheckoutAsync(order);

        // Assert
        receipt.Subtotal.Should().Be(1500m);
        receipt.Discount.Should().Be(100m);
        receipt.Total.Should().Be(1400m);
    }
}