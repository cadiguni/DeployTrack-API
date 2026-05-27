using SampleOrders.Api.Models;

namespace SampleOrders.Api.Dtos;

public sealed record CreateOrderRequest(
    string CustomerName,
    IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    string ProductName,
    int Quantity,
    decimal UnitPrice);

public sealed record UpdateOrderStatusRequest(OrderStatus Status);

public sealed record OrderItemResponse(
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalAmount);

public sealed record OrderResponse(
    Guid Id,
    string CustomerName,
    IReadOnlyList<OrderItemResponse> Items,
    OrderStatus Status,
    decimal TotalAmount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
