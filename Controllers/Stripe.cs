using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Namespace;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models; // Make sure to replace "YourNamespace" with the actual namespace of your models
using Microsoft.EntityFrameworkCore;
namespace YourNamespace.Controllers
{
    [ApiController]
    public class StripeController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Apicontext _context; // Replace ApplicationDbContext with your actual DbContext class

        public StripeController(IConfiguration configuration, Apicontext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet("/stripe")]
        public async Task<IActionResult> Payment()
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:ApiKey"]; // Replace with your configuration key
            Console.WriteLine("Stripe API Key: " + StripeConfiguration.ApiKey);
            List<CartItem> cartItems = _context.CartItems.Include(ci => ci.Product).ToList();
            long orderAmount = (long)CalculateOrderTotal(cartItems);

            var options = new PaymentIntentCreateOptions
            {
                Amount = orderAmount * 100, // Stripe uses cents, so multiply by 100
                Currency = "usd",
            };

            var service = new PaymentIntentService();
            PaymentIntent paymentIntent;
            try
            {
                paymentIntent = await service.CreateAsync(options);
            }
            catch (StripeException e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Payment Intent creation failed");
            }

            return Ok(new { client_secret = paymentIntent.ClientSecret });
        }

        private double CalculateOrderTotal(List<CartItem> cartItems)
        {
            double orderAmount = 0;

            foreach (var cartItem in cartItems)
            {
                // Assuming each cart item corresponds to a product with a 'Price' property
                orderAmount += cartItem.Product.Price * cartItem.Quantity;
            }

            return orderAmount;
        }
    }
}


