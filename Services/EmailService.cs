﻿using System.Net.Mail;
using System.Net;

public class EmailService
{
	private readonly IConfiguration _config;

	public EmailService(IConfiguration config)
	{
		_config = config;
	}

	public async Task SendEmailAsync(string toEmail, string subject, string message)
	{
		var smtpClient = new SmtpClient(_config["EmailSettings:SmtpServer"])
		{
			Port = int.Parse(_config["EmailSettings:SmtpPort"]),
			Credentials = new NetworkCredential(
				_config["EmailSettings:SenderEmail"],
				_config["EmailSettings:SenderPassword"]
			),
			EnableSsl = true
		};

		var mailMessage = new MailMessage
		{
			From = new MailAddress(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderName"]),
			Subject = subject,
			Body = message,
			IsBodyHtml = true
		};
		mailMessage.To.Add(toEmail);

		await smtpClient.SendMailAsync(mailMessage);
	}
}
