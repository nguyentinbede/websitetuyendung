using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TuyenDungFPT.Models;
using SlugGenerator;
using System.Linq.Dynamic.Core;

namespace TuyenDungFPT.Areas.Recruiter.Controllers
{
	[Area("Recruiter")]
	[Authorize(Roles = "Recruiter")]
	public class JobsController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;

		public JobsController(TuyenDungFPTDbContext context)
		{
			_context = context;
		}

		private async Task<int?> GetCurrentUserCompanyIdAsync()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			var user = await _context.Users.FindAsync(userId);
			return user?.CompanyId;
		}

		public IActionResult Index()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Index_LoadData()
		{
			try
			{
				var currentCompanyId = await GetCurrentUserCompanyIdAsync();
				var draw = Request.Form["draw"].FirstOrDefault();
				var start = Request.Form["start"].FirstOrDefault();
				var length = Request.Form["length"].FirstOrDefault();
				var sortColumn = Request.Form[$"columns[{Request.Form["order[0][column]"].FirstOrDefault()}][name]"].FirstOrDefault();
				var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
				var searchValue = Request.Form["search[value]"].FirstOrDefault();

				int pageSize = length != null ? int.Parse(length) : 0;
				int skip = start != null ? int.Parse(start) : 0;

				var jobQuery = _context.Jobs
					.Where(r => r.CompanyId == currentCompanyId)
					.Include(r => r.Company)
					.Include(r => r.JobCategory)
					.Select(r => new
					{
						r.Id,
						NameCompany = r.Company.Name,
						NameJobCategory = r.JobCategory.Name,
						r.Title,
						r.Description,
						r.Requirements,
						r.SalaryRange,
						r.Location,
						r.JobType,
						r.PostedAt,
					});

				if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
				{
					jobQuery = jobQuery.OrderBy(sortColumn + " " + sortColumnDirection);
				}

				if (!string.IsNullOrWhiteSpace(searchValue))
				{
					jobQuery = jobQuery.Where(r => r.Title.Contains(searchValue) ||
												   r.NameCompany.Contains(searchValue) ||
												   r.NameJobCategory.Contains(searchValue) ||
												   r.Description.Contains(searchValue) ||
												   r.Requirements.Contains(searchValue) ||
												   r.SalaryRange.Contains(searchValue) ||
												   r.Location.Contains(searchValue) ||
												   r.JobType.ToString().Contains(searchValue) ||
												   r.PostedAt.ToString().Contains(searchValue));
				}

				var totalRecords = await jobQuery.CountAsync();
				var data = await jobQuery.Skip(skip).Take(pageSize).ToListAsync();

				return Json(new { draw, recordsFiltered = totalRecords, recordsTotal = totalRecords, data });
			}
			catch
			{
				return StatusCode(500);
			}
		}

		public async Task<IActionResult> Details(int? id)
		{
			if (id == null) return NotFound();

			var currentCompanyId = await GetCurrentUserCompanyIdAsync();
			var job = await _context.Jobs.Include(j => j.Company).Include(j => j.JobCategory)
										 .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == currentCompanyId);
			return job == null ? NotFound() : View(job);
		}

		public async Task<IActionResult> Create()
		{
			ViewData["JobCategoryId"] = new SelectList(_context.JobCategories, "Id", "Name");
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,JobCategoryId,Title,Title_khongdau,Description,Requirements,SalaryRange,Location,JobType,PostedAt")] Job job)
		{
			var currentCompanyId = await GetCurrentUserCompanyIdAsync();
			job.CompanyId = currentCompanyId ?? 0;

			if (ModelState.IsValid)
			{
				job.Title_khongdau = string.IsNullOrWhiteSpace(job.Title_khongdau) ? job.Title.GenerateSlug() : job.Title_khongdau;
				_context.Add(job);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["JobCategoryId"] = new SelectList(_context.JobCategories, "Id", "Name", job.JobCategoryId);
			return View(job);
		}

		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null) return NotFound();
			var currentCompanyId = await GetCurrentUserCompanyIdAsync();
			var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == currentCompanyId);
			if (job == null) return Forbid();

			ViewData["JobCategoryId"] = new SelectList(_context.JobCategories, "Id", "Name", job.JobCategoryId);
			return View(job);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,JobCategoryId,Title,Title_khongdau,Description,Requirements,SalaryRange,Location,JobType,PostedAt")] Job job)
		{
			var currentCompanyId = await GetCurrentUserCompanyIdAsync();

			if (id != job.Id) return NotFound();

			if (ModelState.IsValid)
			{
				try
				{
					// Gán CompanyId đúng của user hiện tại
					job.CompanyId = (int)currentCompanyId;

					// Nếu chưa có Title_khongdau thì tạo từ Title
					job.Title_khongdau = string.IsNullOrWhiteSpace(job.Title_khongdau)
						? job.Title.GenerateSlug()
						: job.Title_khongdau;

					_context.Update(job);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!JobExists(job.Id)) return NotFound();
					throw;
				}

				return RedirectToAction(nameof(Index));
			}

			ViewData["JobCategoryId"] = new SelectList(_context.JobCategories, "Id", "Name", job.JobCategoryId);
			return View(job);
		}
		

		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null) return NotFound();

			var currentCompanyId = await GetCurrentUserCompanyIdAsync();
			var job = await _context.Jobs.Include(j => j.Company).Include(j => j.JobCategory)
										 .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == currentCompanyId);
			return job == null ? Forbid() : View(job);
		}

		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var currentCompanyId = await GetCurrentUserCompanyIdAsync();
			var job = await _context.Jobs.FirstOrDefaultAsync(j => j.Id == id && j.CompanyId == currentCompanyId);
			if (job == null) return Forbid();
			// Xóa các đơn ứng tuyển (Applications) của Job này
			var applications = await _context.Applications
				.Where(a => a.JobId == id)
				.ToListAsync();
			_context.Applications.RemoveRange(applications);

			// Xóa các công việc đã lưu (SavedJobs) liên quan
			var savedJobs = await _context.SavedJobs
				.Where(sj => sj.JobId == id)
				.ToListAsync();
			_context.SavedJobs.RemoveRange(savedJobs);

			_context.Jobs.Remove(job);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool JobExists(int id)
		{
			return _context.Jobs.Any(e => e.Id == id);
		}
	}
}
