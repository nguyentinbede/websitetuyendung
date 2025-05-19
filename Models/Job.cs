using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using static System.Net.Mime.MediaTypeNames;

namespace TuyenDungFPT.Models
{
	public class Job
	{
		[DisplayName("Mã công việc")]
		public int Id { get; set; }
		[DisplayName("Tên công ty")]
		[Required(ErrorMessage ="Tên công ty không được bỏ trống")]
		public int CompanyId { get; set; }
		[DisplayName("Danh mục công việc")]
		[Required(ErrorMessage ="Danh mục công việc không được bỏ trống")]
		public int JobCategoryId { get; set; }
		[DisplayName("Tên công việc")]
		[Required(ErrorMessage = "Tên công việc không được bỏ trống")]
		public string Title { get; set; }
		[DisplayName("Tên công việc không dấu")]
		public string? Title_khongdau { get; set; }
		[DisplayName("Mô tả công việc")]
		[Required(ErrorMessage = "Mô tả công việc không được bỏ trống")]
		public string Description { get; set; } // Mô tả
		[DisplayName("Yêu cầu công việc")]
		[Required(ErrorMessage = "Yêu cầu công việc không được bỏ trống")]
		public string Requirements { get; set; } // Yêu cầu
		[DisplayName("Mức lương")]
		[Required(ErrorMessage = "Mức lương không được bỏ trống")]
		public string SalaryRange { get; set; } // Mức lương
		[DisplayName("Địa điểm")]
		[Required(ErrorMessage = "Địa điểm không được bỏ trống")]
		public string Location { get; set; } // Địa điểm
		[DisplayName("Loại công việc")]
		[Required(ErrorMessage = "Loại công việc không được bỏ trống")]
		public JobType JobType { get; set; } // Enum: FullTime, PartTime, Internship, Contract
		[DisplayName("Thời gian đăng công việc")]
		public DateTime PostedAt { get; set; } = DateTime.UtcNow; // ngày đăng
		public Company? Company { get; set; }
		public JobCategory? JobCategory { get; set; }
		public ICollection<Application>? Applications { get; set; }
	}

	public enum JobType
	{
		FullTime,
		PartTime,
		Internship,
		Contract
	}
}
