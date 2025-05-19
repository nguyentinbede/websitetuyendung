using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace TuyenDungFPT.Models
{
	public class JobCategory
	{
		[DisplayName("Mã danh mục công việc")]
		public int Id { get; set; }
		[DisplayName("Tên danh mục công việc")]
		[Required(ErrorMessage ="Tên danh mục công việc không được bỏ trống")]
		public string Name { get; set; }
		public string? Name_khongdau { get; set; }

		public ICollection<Job>? Job { get; set; } 
	}

}
