using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using PayPal.Api;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YourNamespace.Models; // Assuming you have CartItem and Product models
using Namespace;
using Microsoft.EntityFrameworkCore;

namespace YourNamespace.Controllers
{
    [ApiController]
    public class PaypalController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly Apicontext _dbContext;

        public PaypalController(IConfiguration configuration, Apicontext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet("complete")]
        public IActionResult Complete()
        {
            // Implement the logic for completing the PayPal payment
            // For example, you can return a success message
            return Ok("Payment completed successfully");
        }

        [HttpGet("cancel")]
        public IActionResult Cancel()
        {
            // Implement the logic for handling a canceled PayPal payment
            // For example, you can return a cancellation message
            return Ok("Payment canceled");
        }

        [HttpGet("paypal")]
        public async Task<IActionResult> CreatePaymentAsync()
        {
            try
            {
                var clientId = _configuration.GetValue<string>("PayPal:ClientId");
                var clientSecret = _configuration.GetValue<string>("PayPal:ClientSecret");

                var oauthTokenCredential = new OAuthTokenCredential(clientId, clientSecret);
                var accessToken = oauthTokenCredential.GetAccessToken();

                var apiContext = new APIContext(accessToken);

                var cartItems = await _dbContext.CartItems.Include(ci => ci.Product).ToListAsync();
                double orderAmount = CalculateOrderTotal(cartItems);

                var payer = new Payer { payment_method = "paypal" };

                var itemList = new ItemList
                {
                    items = cartItems.Select(cartItem => new Item
                    {
                        name = cartItem.Product.Name,
                        currency = "USD",
                        quantity = cartItem.Quantity.ToString(),
                        price = cartItem.Product.Price.ToString("0.00")
                    }).ToList()
                };

                var amount = new Amount { total = orderAmount.ToString("0.00"), currency = "USD" };

                var transaction = new Transaction
                {
                    amount = amount,
                    item_list = itemList,
                    description = "Purchase from Your Store"
                };

                var redirectUrls = new RedirectUrls
                {
                    return_url = "http://localhost:8080/api/paypal/complete",
                    cancel_url = "http://localhost:8080/api/paypal/cancel"
                };

                var payment = new Payment { intent = "sale", payer = payer };
                payment.transactions = new List<Transaction> { transaction };
                payment.redirect_urls = redirectUrls;

                var createdPayment = payment.Create(apiContext);
                var approvalUrl = createdPayment.links.FirstOrDefault(link => link.rel == "approval_url")?.href;

                return Ok(new { approvalUrl });
            }
            catch (PayPal.PayPalException ex)
            {
                return BadRequest("PayPal API error");
            }
        }

        private double CalculateOrderTotal(List<CartItem> cartItems)
        {
            double orderAmount = 0;

            foreach (var cartItem in cartItems)
            {
                orderAmount += cartItem.Product.Price * cartItem.Quantity;
            }

            return orderAmount;
        }
    }
}
