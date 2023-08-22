using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSender.Controllers
{
    [ApiController]
    public class SendEmailController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SendEmailController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("/send-email")]
        public async Task<IActionResult> SendEmail(
            [FromForm] string to,
            [FromForm] string subject,
            [FromForm] string text)
        {
            try
            {
                string smtpHost = _configuration["SmtpSettings:Host"];
                int smtpPort = int.Parse(_configuration["SmtpSettings:Port"]);
                string smtpUsername = _configuration["SmtpSettings:Username"];
                string smtpPassword = _configuration["SmtpSettings:Password"];

                using (var smtpClient = new SmtpClient(smtpHost, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    smtpClient.EnableSsl = true;

                    var message = new MailMessage
                    {
                        From = new MailAddress(smtpUsername),
                        Subject = subject,
                        Body = text,
                        IsBodyHtml = false
                    };
                    message.To.Add(to);

                    await smtpClient.SendMailAsync(message);
                }

                return Ok("Email sent successfully");
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}
