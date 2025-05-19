using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models;
using TuyenDungFPT.ViewModels;

namespace TuyenDungFPT.Controllers
{
	[Authorize(Roles = "Applicant")]
	public class SavedJobsController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;

		public SavedJobsController(TuyenDungFPTDbContext context)
		{
			_context = context;
		}

		// Hàm này sẽ lưu hoặc bỏ lưu công việc
		[HttpPost]
		public async Task<IActionResult> ToggleSave(int jobId)
		{
			var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

			if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
			{
				return Unauthorized();
			}

			var savedJob = await _context.SavedJobs
				.FirstOrDefaultAsync(sj => sj.JobId == jobId && sj.UserId == userId);

			if (savedJob != null)
			{
				// Nếu công việc đã lưu, xóa khỏi SavedJobs
				_context.SavedJobs.Remove(savedJob);
				await _context.SaveChangesAsync();
				return Json(false); // Gỡ lưu
			}
			else
			{
				// Nếu công việc chưa lưu, thêm vào SavedJobs
				var newSavedJob = new SavedJob
				{
					JobId = jobId,
					UserId = userId,
					SavedAt = DateTime.Now
				};

				_context.SavedJobs.Add(newSavedJob);
				await _context.SaveChangesAsync();
				return Json(true); // Đã lưu
			}
		}

		// Hiển thị danh sách công việc đã lưu của người dùng
		public async Task<IActionResult> MySavedJobs()
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

			var savedJobs = await _context.SavedJobs
				.Include(s => s.Job)
					.ThenInclude(j => j.Company)
				.Where(s => s.UserId == userId)
				.ToListAsync();

			var jobViewModels = savedJobs.Select(s => new JobViewModel
			{
				Job = s.Job,
				IsSaved = true
			}).ToList();

			return View(jobViewModels);
		}
	}
}
