using System.Text.Json.Serialization;
using SampleOrders.Api.Dtos;
using SampleOrders.Api.Models;
using SampleOrders.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddSingleton<IOrderStore, InMemoryOrderStore>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy",
    service = "Sample Orders API",
    checkedAt = DateTimeOffset.UtcNow
}));

var orders = app.MapGroup("/orders");

orders.MapPost("/", (CreateOrderRequest request, IOrderStore store) =>
{
    var validationError = ValidateCreateOrder(request);

    if (validationError is not null)
    {
        return Results.BadRequest(new { message = validationError });
    }

    var order = new Order
    {
        CustomerName = request.CustomerName.Trim(),
        Items = request.Items
            .Select(item => new OrderItem
            {
                ProductName = item.ProductName.Trim(),
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            })
            .ToList()
    };

    store.Create(order);

    return Results.Created($"/orders/{order.Id}", ToResponse(order));
});

orders.MapGet("/", (IOrderStore store) =>
    Results.Ok(store.GetAll().Select(ToResponse)));

orders.MapGet("/{id:guid}", (Guid id, IOrderStore store) =>
{
    var order = store.GetById(id);

    return order is null ? Results.NotFound() : Results.Ok(ToResponse(order));
});

orders.MapPatch("/{id:guid}/status", (Guid id, UpdateOrderStatusRequest request, IOrderStore store) =>
{
    var order = store.UpdateStatus(id, request.Status);

    return order is null ? Results.NotFound() : Results.Ok(ToResponse(order));
});

app.Run();

static string? ValidateCreateOrder(CreateOrderRequest request)
{
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        return "CustomerName is required.";
    }

    if (request.Items is null || request.Items.Count == 0)
    {
        return "At least one item is required.";
    }

    foreach (var item in request.Items)
    {
        if (string.IsNullOrWhiteSpace(item.ProductName))
        {
            return "ProductName is required for all items.";
        }

        if (item.Quantity <= 0)
        {
            return "Quantity must be greater than zero.";
        }

        if (item.UnitPrice < 0)
        {
            return "UnitPrice cannot be negative.";
        }
    }

    return null;
}

static OrderResponse ToResponse(Order order) =>
    new(
        order.Id,
        order.CustomerName,
        order.Items
            .Select(item => new OrderItemResponse(
                item.ProductName,
                item.Quantity,
                item.UnitPrice,
                item.Quantity * item.UnitPrice))
            .ToList(),
        order.Status,
        order.TotalAmount,
        order.CreatedAt,
        order.UpdatedAt);
