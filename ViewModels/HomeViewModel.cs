using System.Collections.Generic;
using TuyenDungFPT.Models;
using TuyenDungFPT.ViewModels;

namespace TuyenDungFPT.Models.ViewModels
{
	public class HomeViewModel
	{
		public List<JobCategory> Categories { get; set; }
		public List<JobViewModel> Jobs { get; set; }
	}
}
