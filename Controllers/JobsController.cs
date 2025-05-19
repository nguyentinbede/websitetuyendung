using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TuyenDungFPT.Models;
using TuyenDungFPT.ViewModels;

namespace TuyenDungFPT.Controllers
{
	public class JobsController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;

		public JobsController(TuyenDungFPTDbContext context)
		{
			_context = context;
		}

		// GET: /Jobs/TheoLoai?tenLoai=ten-khong-dau
		public async Task<IActionResult> JobType()
		{
			var jobs = await _context.Jobs
				.Include(j => j.JobCategory)
				.Include(j => j.Company)
				.ToListAsync();

			var savedJobIds = new List<int>();
			if (User.Identity.IsAuthenticated)
			{
				var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
				if (int.TryParse(userIdStr, out int userId))
				{
					savedJobIds = await _context.SavedJobs
						.Where(sj => sj.UserId == userId)
						.Select(sj => sj.JobId)
						.ToListAsync();
				}
			}

			var jobViewModels = jobs.Select(job => new JobViewModel
			{
				Job = job,
				IsSaved = savedJobIds.Contains(job.Id)
			}).ToList();

			return View(jobViewModels);
		}


		public async Task<IActionResult> Details(int id)
		{
			var job = await _context.Jobs
				.Include(j => j.Company)
				.FirstOrDefaultAsync(j => j.Id == id);

			if (job == null)
			{
				return NotFound();
			}

			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (int.TryParse(userIdStr, out int userId))
			{
				var alreadyApplied = await _context.Applications
					.AnyAsync(a => a.JobId == id && a.UserId == userId);
				ViewBag.AlreadyApplied = alreadyApplied;
			}
			else
			{
				ViewBag.AlreadyApplied = false;
			}

			return View(job);
		}

		[HttpGet]
		public async Task<IActionResult> Search(string keyword, string categoryId, string location)
		{
			int? catId = null;
			if (!string.IsNullOrEmpty(categoryId) && categoryId != "Category")
			{
				catId = int.Parse(categoryId);
			}

			var jobsQuery = _context.Jobs
				.Include(j => j.Company)
				.Include(j => j.JobCategory)
				.AsQueryable();

			if (!string.IsNullOrEmpty(keyword))
			{
				jobsQuery = jobsQuery.Where(j =>
					j.Title.ToLower().Contains(keyword.ToLower()) ||
					j.Description.ToLower().Contains(keyword.ToLower()));
			}

			if (catId.HasValue)
			{
				jobsQuery = jobsQuery.Where(j => j.JobCategoryId == catId.Value);
			}

			if (!string.IsNullOrEmpty(location) && location != "Location")
			{
				jobsQuery = jobsQuery.Where(j => j.Location.ToLower().Contains(location.ToLower()));
			}

			var jobList = await jobsQuery
				.OrderByDescending(j => j.PostedAt)
				.Select(j => new JobViewModel
				{
					Job = j,
					IsSaved = false
				})
				.ToListAsync();

			return View("SearchResult", jobList);
		}


	}
}
