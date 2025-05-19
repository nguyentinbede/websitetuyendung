using TuyenDungFPT.Models;

namespace TuyenDungFPT.ViewModels
{
	public class SearchViewModel
	{
		public string? Keyword { get; set; }
		public int? JobCategoryId { get; set; }
		public string? Location { get; set; }
		public List<JobCategory>? Categories { get; set; }
		public List<string>? Locations { get; set; }
	}
}
