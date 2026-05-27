using SampleOrders.Api.Models;

namespace SampleOrders.Api.Services;

public interface IOrderStore
{
    IReadOnlyList<Order> GetAll();
    Order? GetById(Guid id);
    Order Create(Order order);
    Order? UpdateStatus(Guid id, OrderStatus status);
}
