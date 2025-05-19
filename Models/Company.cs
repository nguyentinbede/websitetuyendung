using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TuyenDungFPT.Models
{
	public class Company
	{
		[DisplayName("Mã công ty")]
		public int Id { get; set; }
		[DisplayName("Tên công ty")]
		[Required(ErrorMessage ="Tên công ty không được để trống")]
		public string Name { get; set; }
		[DisplayName("Tên công ty không dấu")]
		public string? Name_khongdau { get; set; }
		[DisplayName("Logo công ty")]
		public string? Avata {  get; set; }
		[NotMapped]
		[Display(Name = "Logo công ty")]
		public IFormFile? DataAvata { get; set; }
		[DisplayName("Website công ty")]
		[Required(ErrorMessage = "Website công ty không được để trống")]

		public string Website { get; set; }
		[DisplayName("Chuyên ngành công ty")]
		[Required(ErrorMessage = "Chuyên ngành công ty không được để trống")]
		public string Industry { get; set; } // Ngành nghề
		[DisplayName("Địa điểm công ty")]
		[Required(ErrorMessage = "Địa điểm công ty không được để trống")]
		public string Location { get; set; } // Địa điểm
		[DisplayName("Mô tả công ty")]
		[Required(ErrorMessage = "Mô tả công ty không được để trống")]
		public string Description { get; set; } // Mô tả

		public ICollection<Job>? Jobs { get; set; }
		public ICollection<User>? User { get; set; }
	}

}
