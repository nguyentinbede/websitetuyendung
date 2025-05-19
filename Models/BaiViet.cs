using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace TuyenDungFPT.Models
{
	public class BaiViet
	{
		[DisplayName("Mã BV")]
		public int Id { get; set; }

		[DisplayName("Chủ đề")]
		[Required(ErrorMessage = "Chủ đề không được bỏ trống")]
		public int ChuDeId { get; set; }

		[DisplayName("Người đăng")]
		[Required(ErrorMessage = "Người đăng không được bỏ trống.")]
		public int UserId { get; set; }

		[StringLength(255)]
		[DisplayName("Tiêu đề")]
		[Required(ErrorMessage = "Tiêu đề không được bỏ trống")]
		public string TieuDe { get; set; }

		[StringLength(255)]
		[DisplayName("Tiêu đề không dấu")]
		public string? TieuDeKhongDau { get; set; }

		[Column(TypeName = "ntext")]
		[DisplayName("Tóm tắt bài viết")]
		[Required(ErrorMessage = "Tóm tắt bài viết không được bỏ trống.")]
		[DataType(DataType.MultilineText)]
		public string TomTat { get; set; }

		[Column(TypeName = "ntext")]
		[DisplayName("Nội dung")]
		[Required(ErrorMessage = "Nội dung bài viết không được bỏ trống.")]
		[DataType(DataType.MultilineText)]
		public string NoiDung { get; set; }

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

		[DisplayName("Hiển thị")]
		[DefaultValue(true)]
		public bool HienThi { get; set; }

		public ChuDe? ChuDe { get; set; }
		public User? User { get; set; }
		public ICollection<BinhLuanBaiViet>? BinhLuanBaiViet { get; set; }
	}
	[NotMapped]
	public class PhanTrangBaiViet
	{
		public int TrangHienTai { get; set; }
		public int TongSoTrang { get; set; }
		public List<BaiViet> BaiViet { get; set; }
		public bool HasPreviousPage => TrangHienTai > 1;
		public bool HasNextPage => TrangHienTai < TongSoTrang;
	}

}
