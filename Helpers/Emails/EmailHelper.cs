using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;

namespace SchoolSystem.Helpers.Emails
{
    public class EmailHelper : IEmailHelper
    {
        private readonly IConfiguration _configuration;

        public EmailHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Method to send an email
        public Response SendEmail(string to, string subject, string body)
        {
            // Retrieve email configuration settings from appsettings.json
            var nameFrom = _configuration["Mail:NameFrom"];
            var from = _configuration["Mail:From"];
            var smtp = _configuration["Mail:Smtp"];
            var port = _configuration["Mail:Port"];
            var password = _configuration["Mail:Password"];

            // Create a new MimeMessage
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(nameFrom, from));
            message.To.Add(new MailboxAddress(to, to));
            message.Subject = subject;

            // Set the email body as HTML
            var bodybuilder = new BodyBuilder { HtmlBody = body };
            message.Body = bodybuilder.ToMessageBody();

            try
            {
                // Connect to the SMTP server and send the email
                using (var client = new SmtpClient())
                {
                    client.Connect(smtp, int.Parse(port), false);
                    client.Authenticate(from, password);
                    client.Send(message);
                    client.Disconnect(true);
                }
            }
            catch (Exception ex)
            {
                // If an exception occurs, return a failure response with the exception message
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                };
            }

            // Return a success response
            return new Response
            {
                IsSuccess = true
            };
        }
    }
}
