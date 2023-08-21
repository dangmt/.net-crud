using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YourNamespace.Models; // Make sure to include your model classes here
using Namespace;

namespace YourNamespace.Controllers
{
    //[Route("api/[controller]")]

    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly Apicontext _context;

        public OrderController(Apicontext context)
        {
            _context = context;
        }

        [HttpGet("/orders")]
        public ActionResult<IEnumerable<Order>> GetAllOrders()
        {
            List<Order> orders = _context.Orders.Include(o => o.OrderItems).ToList();
            return Ok(orders);
        }

        [HttpPost("/orders")]
        [ProducesResponseType(200)]
        public ActionResult CreateOrder()
        {
            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    Order order = new Order();
                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    List<CartItem> cartItems = _context.CartItems.Include(ci => ci.Product).ToList();

                    foreach (CartItem cartItem in cartItems)
                    {
                        OrderItem orderItem = new OrderItem
                        {
                            OrderId = order.Id,
                            ProductId = cartItem.ProductId,
                            Quantity = cartItem.Quantity
                        };
                        _context.OrderItems.Add(orderItem);
                        _context.SaveChanges();


                    }
                    transaction.Commit();
                    return Ok("Order created successfully");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpGet("/orders/{orderId}")]
        public ActionResult<Order> GetOrder(long orderId)
        {
            Order order = _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }

        [HttpPut("/orders/{orderId}")]
        public ActionResult UpdateOrder(long orderId)
        {
            Order existingOrder = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (existingOrder == null)
            {
                return NotFound();
            }

            // Update order properties if needed
            // Example: existingOrder.Status = updatedOrder.Status;
            // _context.SaveChanges();

            return Ok("Order updated successfully");
        }

        [HttpDelete("/orders/{orderId}")]
        public ActionResult DeleteOrder(long orderId)
        {
            Order order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            using (var transaction = _context.Database.BeginTransaction())
            {
                try
                {
                    // Delete associated order items
                    _context.OrderItems.RemoveRange(order.OrderItems);

                    // Delete the order
                    _context.Orders.Remove(order);
                    _context.SaveChanges();

                    transaction.Commit();
                    return Ok("Order deleted successfully");
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
        [HttpDelete("/orders")]
        public ActionResult DeleteOrder()
        {
            List<Order> ordersToDelete = _context.Orders.ToList();

            // Remove orders from the context
            _context.Orders.RemoveRange(ordersToDelete);

            // Save changes to delete orders
            _context.SaveChanges();


            return Ok("Orders deleted successfully");

        }
    }
}

