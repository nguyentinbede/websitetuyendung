using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using Newtonsoft.Json;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.Design;
using static System.Net.Mime.MediaTypeNames;

namespace TuyenDungFPT.Models
{
	public class User 
	{
		[DisplayName("Mã người dùng")]
		public int Id { get; set; }
		[DisplayName("Công ty của người tuyển dụng")]
		public int? CompanyId { get; set; }
		[Required(ErrorMessage = "Họ tên không được bỏ trống")]
		[DisplayName("Họ và tên")]
		public string FullName { get; set; }
		[DisplayName("Họ và tên không dấu")]
		public string? FullName_khongdau { get; set; }
		[Required(ErrorMessage = "Email không được bỏ trống")]
		[DisplayName("Email")]
		public string Email { get; set; }
		[Required(ErrorMessage = "Mật khẩu không được bỏ trống")]
		[DataType(DataType.Password)]
		[DisplayName("Mật khẩu")]
		public string PasswordHash { get; set; }
		[NotMapped]
		[Display(Name = "Xác nhận mật khẩu")]
		[Required(ErrorMessage = "Xác nhận mật khẩu không được bỏ trống!")]
		[Compare("PasswordHash", ErrorMessage = "Xác nhận mật khẩu không chính xác!")]
		[DataType(DataType.Password)]
		public string XacNhanMatKhau { get; set; }
		[DisplayName("Số điện thoại")]
		public string? Phone { get; set; }
		[DisplayName("Ảnh đại diện")]
		public string? Avata { get; set; }
		[NotMapped]
		[Display(Name = "Ảnh đại diện")]
		public IFormFile? DataAvata { get; set; }
		[Required(ErrorMessage = "Quyền không được bỏ trống")]
		[DisplayName("Quyền")]
		public UserRole Role { get; set; }  // Enum: Applicant, Recruiter, Admin
		[DisplayName("Thời giạn tạo")]
		public bool EmailConfirmed { get; set; } = false;

		public string? ConfirmationToken { get; set; }
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public Company? Company { get; set; }

		public ICollection<Application>? Applications { get; set; }
		public ICollection<Resume>? Resumes { get; set; }
		public ICollection<SavedJob>? SavedJobs { get; set; }
		public ICollection<BaiViet>? BaiViet { get; set; }
		public ICollection<BinhLuanBaiViet>? BinhLuanBaiViet { get; set; }

	}

	public enum UserRole
	{
		Applicant,  // Ứng viên
		Recruiter,  // Nhà tuyển dụng
		Admin       // Quản trị viên
	}
	[NotMapped]
	public class NguoiDung_ChinhSua
	{
		public NguoiDung_ChinhSua()
		{

		}

		public NguoiDung_ChinhSua(User n)
		{
			Id = n.Id;
			CompanyId = n.CompanyId;
			FullName = n.FullName;
			Email = n.Email;
			Phone = n.Phone;
			PasswordHash = n.PasswordHash;
			XacNhanMatKhau = n.XacNhanMatKhau;
			Avata = n.Avata;
			DataAvata = n.DataAvata;
			Role = n.Role;
			CreatedAt = n.CreatedAt;
		}
		[DisplayName("Mã người dùng")]
		public int Id { get; set; }
		[DisplayName("Công ty của người tuyển dụng")]
		public int? CompanyId { get; set; }
		[DisplayName("Họ và tên")]

		public string FullName { get; set; }
		[DisplayName("Họ và tên không dấu")]
		public string? FullName_khongdau { get; set; }
		[Required(ErrorMessage = "Email không được bỏ trống")]
		[DisplayName("Email")]
		public string Email { get; set; }
		[DataType(DataType.Password)]
		[DisplayName("Mật khẩu")]
		public string? PasswordHash { get; set; }
		[NotMapped]
		[Display(Name = "Xác nhận mật khẩu")]
		[Compare("PasswordHash", ErrorMessage = "Xác nhận mật khẩu không chính xác!")]
		[DataType(DataType.Password)]
		public string? XacNhanMatKhau { get; set; }
		[Required(ErrorMessage = "Số điện thoại không được bỏ trống")]
		[DisplayName("Số điện thoại")]
		public string? Phone { get; set; }
		[DisplayName("Ảnh đại diện")]
		public string? Avata { get; set; }
		[NotMapped]
		[Display(Name = "Ảnh đại diện")]
		public IFormFile? DataAvata { get; set; }
		[Required(ErrorMessage = "Quyền không được bỏ trống")]
		[DisplayName("Quyền")]
		public UserRole Role { get; set; }  // Enum: Applicant, Recruiter, Admin
		[DisplayName("Thời giạn tạo")]
		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	}
	[NotMapped]
	public class DangNhap
	{
		[Required(ErrorMessage = "Email đăng nhập không được bỏ trống!")]
		[DisplayName("Email")]
		public string Email { get; set; }
		[DataType(DataType.Password)]
		[Required(ErrorMessage = "Mật khẩu không được bỏ trống!")]
		[DisplayName("Mật khẩu")]
		public string PasswordHash { get; set; }
		[DisplayName("Duy trì đăng nhập")]
		public bool DuyTriDangNhap { get; set; }
		[StringLength(255)]
		[DisplayName("Liên kết chuyển trang")]
		public string? LienKetChuyenTrang { get; set; }
	}
}
