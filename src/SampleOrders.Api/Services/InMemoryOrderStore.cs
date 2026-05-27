using System.Collections.Concurrent;
using SampleOrders.Api.Models;

namespace SampleOrders.Api.Services;

public sealed class InMemoryOrderStore : IOrderStore
{
    private readonly ConcurrentDictionary<Guid, Order> orders = new();

    public IReadOnlyList<Order> GetAll() =>
        orders.Values.OrderByDescending(order => order.CreatedAt).ToList();

    public Order? GetById(Guid id) =>
        orders.GetValueOrDefault(id);

    public Order Create(Order order)
    {
        orders[order.Id] = order;
        return order;
    }

    public Order? UpdateStatus(Guid id, OrderStatus status)
    {
        if (!orders.TryGetValue(id, out var order))
        {
            return null;
        }

        order.Status = status;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        return order;
    }
}
