using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TuyenDungFPT.Models;

namespace TuyenDungFPT.ViewModels
{
	public class ProfileEditViewModel
	{
		public int Id { get; set; }

		[Required(ErrorMessage = "Họ tên không được bỏ trống")]
		[DisplayName("Họ và tên")]
		public string FullName { get; set; }

		[DisplayName("Họ và tên không dấu")]
		public string? FullName_khongdau { get; set; }

		[Required(ErrorMessage = "Số điện thoại không được bỏ trống")]
		[DisplayName("Số điện thoại")]
		public string Phone { get; set; }

		[DisplayName("Ảnh đại diện")]
		public string? Avata { get; set; }

		[NotMapped]
		[DisplayName("Tải ảnh đại diện mới")]
		public IFormFile? DataAvata { get; set; }
		[DataType(DataType.Password)]
		[DisplayName("Mật khẩu")]
		public string? PasswordHash { get; set; }
		[NotMapped]
		[Display(Name = "Xác nhận mật khẩu")]
		[Compare("PasswordHash", ErrorMessage = "Xác nhận mật khẩu không chính xác!")]
		[DataType(DataType.Password)]
		public string? XacNhanMatKhau { get; set; }
		public List<Resume> Resumes { get; set; } = new();
	}
}
