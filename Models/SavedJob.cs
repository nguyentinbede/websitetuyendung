using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TuyenDungFPT.Models
{
	public class SavedJob
	{
		[Required(ErrorMessage = "Họ tên người dùng không được bỏ trống")]
		[DisplayName("Tên người dùng")]
		public int UserId { get; set; }
		[Required(ErrorMessage = " Tên công việc không được bỏ trống")]
		[DisplayName("Tên công việc")]
		public int JobId { get; set; }
		[DisplayName("Thời gian thích công việc")]
		public DateTime SavedAt { get; set; } = DateTime.UtcNow;

		public User User { get; set; }
		public Job Job { get; set; }
	}
}
