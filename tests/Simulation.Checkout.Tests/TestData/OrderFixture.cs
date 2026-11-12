
using AutoFixture;
using Simulation.Checkout.Domain;

namespace Simulation.Checkout.Tests.TestData;

public static class OrderFixture
{
    public static Order CreateValidOrder()
    {
        var fixture = new Fixture();

        var items = fixture
            .Build<OrderItem>()
            .With(x => x.UnitPrice, 25m)
            .With(x => x.Quantity, 2)
            .CreateMany(2)
            .ToList();

        return fixture
            .Build<Order>()
            .With(x => x.Items, items)
            .Create();
    }
}
