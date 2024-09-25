using System;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using SchoolSystem.Helpers.Emails;

namespace SchoolWeb.Helpers
{
	public class EmailHelper : IEmailHelper
	{
		private readonly IConfiguration _configuration;

		public EmailHelper(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public Response SendEmail(string to, string subject, string body)
		{
			var nameFrom = _configuration["Mail:NameFrom"];
			var from = _configuration["Mail:From"];
			var smtp = _configuration["Mail:Smtp"];
			var port = _configuration["Mail:Port"];
			var password = _configuration["Mail:Password"];

			// Check for null or empty configuration values
			if (string.IsNullOrEmpty(nameFrom) || string.IsNullOrEmpty(from) || string.IsNullOrEmpty(smtp) || string.IsNullOrEmpty(port) || string.IsNullOrEmpty(password))
			{
				return new Response
				{
					IsSuccess = false,
					Message = "One or more email configuration settings are missing or empty."
				};
			}

			var message = new MimeMessage();
			message.From.Add(new MailboxAddress(nameFrom, from));
			message.To.Add(new MailboxAddress(to, to));
			message.Subject = subject;

			var bodybuilder = new BodyBuilder { HtmlBody = body };
			message.Body = bodybuilder.ToMessageBody();

			try
			{
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
				Console.WriteLine($"Email sending failed: {ex.Message}");

				return new Response
				{
					IsSuccess = false,
					Message = ex.ToString()
				};
			}

			return new Response
			{
				IsSuccess = true
			};
		}
	}
}
