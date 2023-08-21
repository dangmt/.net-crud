using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models; // Make sure to replace with your namespace
using Namespace;

namespace YourNamespace.Controllers
{
    //[Route("api/[controller]")]
    [ApiController]
    public class CartItemController : ControllerBase
    {
        private readonly Apicontext _context;

        public CartItemController(Apicontext context)
        {
            _context = context;
        }

        [HttpPost("/cartitems")]
        public async Task<IActionResult> StoreCartItem([FromForm] long productId, [FromForm] int quantity)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return NotFound("Product not found");
            }

            var cartItem = await _context.CartItems.FirstOrDefaultAsync(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
                await _context.SaveChangesAsync();
                return Ok("Product quantity updated in cart");
            }
            else
            {
                cartItem = new CartItem
                {
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
                return Ok("Product added to cart");
            }
        }


        [HttpGet("/cartitems/{cartItemId}")]
        public async Task<IActionResult> GetCartItem([FromRoute] long cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }
            return Ok(cartItem);
        }

        [HttpPut("/cartitems/{cartItemId}")]
        public async Task<IActionResult> UpdateCartItem([FromRoute] long cartItemId, [FromForm] int quantity)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return Ok(cartItem);
        }

        [HttpDelete("/cartitems/{cartItemId}")]
        public async Task<IActionResult> DeleteCartItem([FromRoute] long cartItemId)
        {
            var cartItem = await _context.CartItems.FindAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet("/cartitems")]
        public async Task<IActionResult> GetAllCartItems()
        {
            var cartItems = await _context.CartItems.ToListAsync();
            return Ok(cartItems);
        }
    }

}
