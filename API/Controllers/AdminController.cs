using API.DTOs;
using API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Entities.OrderAggregates;
using Core.Interfaces;
using Core.Specifications;

namespace API.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController(IUnitOfWork unit, IPaymentService paymentService) : BaseApiController
{
    [HttpGet("orders")]
    public async Task<ActionResult<IReadOnlyList<OrderDto>>> GetOrders([FromQuery]OrderSpecParams specParams)
    {
        var spec = new OrderSpecification(specParams);
        return await CreatePagedResult(unit.Repository<Order>(), spec, specParams.PageIndex, specParams.PageSize, o => o.ToDto());
    }

    [HttpGet("orders/{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrderById([FromRoute] int id)
    {
        var spec = new OrderSpecification(id);
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        
        if (order == null) return BadRequest("Order not found");
        return order.ToDto();
    }

    [HttpPost("orders/refund/{id:int}")]
    public async Task<ActionResult<OrderDto>> RefundOrder(int id)
    {
        var spec = new OrderSpecification(id);
        
        var order = await unit.Repository<Order>().GetEntityWithSpec(spec);
        
        if (order == null) return BadRequest("Order not found");
        
        if (order.Status == OrderStatus.Pending) return BadRequest("Payment is not received");

        var result = await paymentService.RefundPayment(order.PaymentIntentId);

        if (result == "succeeded")
        {
            order.Status = OrderStatus.Refunded;

            await unit.Complete();
            
            return order.ToDto();

        }
        return BadRequest("Problem refunding payment");
    }
}