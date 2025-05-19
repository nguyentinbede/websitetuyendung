using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TuyenDungFPT.Models
{
	public class Application
	{
		[DisplayName("Mã ứng viên")]
		public int Id { get; set; }
		[DisplayName("Tên công việc")]
		[Required(ErrorMessage ="Tên công việc không được bỏ trống")]
		public int JobId { get; set; }
		[DisplayName("Tên ứng viên")]
		[Required(ErrorMessage = "Tên ứng viên không được bỏ trống")]
		public int UserId { get; set; }
		[DisplayName("CV của ứng viên")]
		[Required(ErrorMessage = "CV của ứng viên không được bỏ trống")]
		public int ResumeId { get; set; }
		[DisplayName("Tình trạng")]
		[Required(ErrorMessage = "Tình trạng không được bỏ trống")]
		public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
		[DisplayName("Ngày ứng tuyển")]
		public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

		public Job? Job { get; set; }
		public User? User { get; set; }
		public Resume? Resume { get; set; }
	}

	public enum ApplicationStatus
	{
		Pending,
		Reviewed,
		Accepted,
		Rejected
	}
}
