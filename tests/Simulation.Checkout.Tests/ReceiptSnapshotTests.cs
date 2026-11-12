
using Moq;
using Simulation.Checkout.Application;
using Simulation.Checkout.Application.Abstractions;
using Simulation.Checkout.Domain;
using VerifyXunit;

namespace Simulation.Checkout.Tests;

public class ReceiptSnapshotTests
{
    [Fact]
    public async Task CheckoutAsync_WhenUsingExampleC_ShouldMatchReceiptSnapshot()
    {
        // Arrange
        var orderRepository = new Mock<IOrderRepository>();
        var inventoryService = new Mock<IInventoryService>();
        var notificationService = new Mock<INotificationService>();

        inventoryService
            .Setup(x => x.IsAvailableAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        orderRepository
            .Setup(x => x.SaveAsync(
                It.IsAny<Order>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        notificationService
            .Setup(x => x.SendOrderConfirmationAsync(
                It.IsAny<CheckoutReceipt>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var sut = new CheckoutService(
            orderRepository.Object,
            inventoryService.Object,
            notificationService.Object);

        var order = new Order
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CustomerName = "Test Customer",
            CustomerEmail = "test@example.com",
            Items = new List<OrderItem>
            {
                new()
                {
                    ProductId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    ProductName = "Product C",
                    UnitPrice = 500m,
                    Quantity = 3
                }
            }
        };

        // Act
        var receipt = await sut.CheckoutAsync(order);

        // Assert
        var settings = new VerifySettings();
        settings.IgnoreMember(nameof(CheckoutReceipt.CreatedAt));
        settings.IgnoreMember(nameof(CheckoutReceipt.OrderId));

        await Verify(receipt, settings);
    }
}
