using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace TuyenDungFPT.Models
{
	public class BinhLuanBaiViet
	{
		[DisplayName("Mã BL")]
		public int Id { get; set; }

		[DisplayName("Bài viết")]
		[Required(ErrorMessage = "Bài viết không được bỏ trống")]
		public int BaiVietId { get; set; }

		[DisplayName("Người đăng")]
		[Required(ErrorMessage = "Người đăng không được bỏ trống.")]
		public int UserId { get; set; }

		[Column(TypeName = "ntext")]
		[DisplayName("Nội dung bình luận")]
		[Required(ErrorMessage = "Nội dung bình luận không được bỏ trống.")]
		[DataType(DataType.MultilineText)]
		public string NoiDungBinhLuan { get; set; }

		[DisplayName("Ngày đăng")]
		[Required(ErrorMessage = "Ngày đăng không được bỏ trống.")]
		[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = false)]
		public DateTime NgayDang { get; set; }

		[DisplayName("Lượt xem")]
		[DefaultValue(1)]
		public int LuotXem { get; set; }

		[DisplayName("Kiểm duyệt")]
		[DefaultValue(false)]
		public bool KiemDuyet { get; set; }

		public BaiViet? BaiViet { get; set; }
		public User? User { get; set; }
	}
}
