
using Bogus;

namespace Simulation.Checkout.Tests.TestData;

public record TestCustomer(string Name, string Email);

public static class CustomerFaker
{
    public static TestCustomer Create()
    {
        var faker = new Faker<TestCustomer>("en")
            .UseSeed(12345)
            .CustomInstantiator(f => new TestCustomer(
                f.Name.FullName(),
                f.Internet.Email()));

        return faker.Generate();
    }
}
