﻿using System.ComponentModel.DataAnnotations;

namespace TuyenDungFPT.ViewModels
{
	public class ContactFormViewModel
	{
		[Required]
		public string Name { get; set; }

		[Required, EmailAddress]
		public string Email { get; set; }

		public string Subject { get; set; }

		[Required]
		public string Message { get; set; }
	}
}
