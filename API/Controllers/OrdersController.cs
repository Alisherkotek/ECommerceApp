using API.DTOs;
using API.Extensions;
using Core.Entities;
using Core.Entities.OrderAggregates;
using Core.Interfaces;
using Core.Specifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Order = Core.Entities.OrderAggregates.Order;
using Product = Core.Entities.Product;

namespace API.Controllers;

[Authorize]
public class OrdersController(ICartService cartService, IUnitOfWork unit) : BaseApiController
{
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto orderDto)
    {
        var email = User.GetEmail();

        var cart = await cartService.GetCartAsync(orderDto.CartId);
        
        if (cart == null) return BadRequest("Cart not found");
        
        if (cart.PaymentIntentId == null) return BadRequest("Cart payment intent");
        
        var items = new List<OrderItem>();

        foreach (var item in cart.Items)
        {
            var productItem = await unit.Repository<Product>().GetByIdAsync(item.ProductId);
            
            if (productItem == null) return BadRequest("Product not found");

            var itemOrdered = new ProductItemOrdered
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                PictureUrl = productItem.PictureUrl,
            };

            var orderItem = new OrderItem
            {
                ItemOrdered = itemOrdered,
                Price = productItem.Price,
                Quantity = item.Quantity,
            };
            items.Add(orderItem);
        }
        
        var deliveryMethod = await unit.Repository<DeliveryMethod>().GetByIdAsync(orderDto.DeliveryMethodId);
        
        if (deliveryMethod == null) return BadRequest("Delivery Method not found");

        var order = new Order
        {
            OrderItems = items,
            DeliveryMethod = deliveryMethod,
            ShippingAddress = orderDto.ShippingAddress,
            Subtotal = items.Sum(x => x.Price * x.Quantity),
            Discount = orderDto.Discount,
            PaymentSummary = orderDto.PaymentSummary,
            PaymentIntentId = cart.PaymentIntentId,
            BuyerEmail = email
        };
        
        unit.Repository<Order>().Add(order);

        if (await unit.Complete())
        {
            return order;
        }
        return BadRequest("Failed to complete order");
    }

    [HttpGet]
    public async Task<ActionResult<List<OrderDto>>> GetOrdersForUser()
    {
         var spec = new OrderSpecification(User.GetEmail());
         
         var orders = await unit.Repository<Order>().ListAsync(spec);

         var ordersToReturn = orders.Select(o => o.ToDto()).ToList();
         
         return Ok(ordersToReturn);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrderById(int id)
    {
        var spec = new OrderSpecification(User.GetEmail(), id);
        
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        
        if (order == null) return BadRequest("Order not found");

        return order.ToDto();
    }
}   