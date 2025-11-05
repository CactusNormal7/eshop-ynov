using Ordering.Domain.Abstractions;
using Ordering.Domain.ValueObjects.Types;

namespace Ordering.Domain.Models;

/// <summary>
/// Represents an item within an order.
/// This model ties a product to a specific order and contains details about the price and quantity of the product.
/// </summary>
public class OrderItem : Entity<OrderItemId>
{
    public ProductId ProductId { get; set; }

    public OrderId OrderId { get; set; }

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public string  Name { get; set; }

    internal OrderItem(ProductId productId, OrderId orderId, decimal price, int quantity, string name )
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        ProductId = productId;
        OrderId = orderId;
        Name = name;
        Price = price;
        Quantity = quantity;
    }
}