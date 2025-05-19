using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuyenDungFPT.Models
{
	public class Resume
	{
		[DisplayName("Mã hồ sơ")]
		public int Id { get; set; }
		[DisplayName("Họ tên")]
		[Required(ErrorMessage ="Họ tên không được bỏ trống")]
		public int UserId { get; set; }
		[DisplayName("File CV")]
		public string? FilePath { get; set; }
		[DisplayName("Ngày tải CV")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		[NotMapped]
		[Display(Name = "File CV")]
		public IFormFile? DataFilePath { get; set; }


		public User? User { get; set; }
	}
}
