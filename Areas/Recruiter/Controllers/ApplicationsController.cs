using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using TuyenDungFPT.Models;

namespace TuyenDungFPT.Areas.Recruiter.Controllers
{
	[Area("Recruiter")]
	[Authorize(Roles = "Recruiter")]
	public class ApplicationsController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;
		private readonly EmailService _emailService;

		public ApplicationsController(TuyenDungFPTDbContext context, EmailService emailService)
		{
			_context = context;
			_emailService = emailService;
		}

		private async Task<int?> GetCurrentUserCompanyIdAsync()
		{
			var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			var user = await _context.Users.FindAsync(userId);
			return user?.CompanyId;
		}

		// Xem danh sách ứng viên ứng tuyển vào các công việc của công ty mình
		public async Task<IActionResult> CompanyApplicants()
		{
			var companyId = await GetCurrentUserCompanyIdAsync();
			if (companyId == null)
			{
				return Forbid();
			}

			var applications = await _context.Applications
				.Include(a => a.User)
				.Include(a => a.Job)
				.ThenInclude(j => j.Company)
				.Include(a => a.Resume)
				.Where(a => a.Job.CompanyId == companyId)
				.ToListAsync();

			return View(applications);
		}
		public async Task<IActionResult> Details(int id)
		{
			var application = await _context.Applications
				.Include(a => a.User)
				.Include(a => a.Job)
				.Include(a => a.Resume) 
				.FirstOrDefaultAsync(a => a.Id == id);

			if (application == null)
			{
				return NotFound();
			}

			// Nếu trạng thái hiện tại là Pending thì chuyển sang Reviewed
			if (application.Status == ApplicationStatus.Pending)
			{
				application.Status = ApplicationStatus.Reviewed;
				_context.Applications.Update(application);
				await _context.SaveChangesAsync();
			}

			return View(application);
		}

		[HttpPost]
		public async Task<IActionResult> UpdateStatus(int id, string status)
		{
			var application = await _context.Applications
				.Include(a => a.User)
				.Include(a => a.Job)
				.Include(a => a.Resume)
				.FirstOrDefaultAsync(a => a.Id == id);

			if (application == null)
				return NotFound();

			string statusText = "";
			switch (status?.ToLower())
			{
				case "accept":
					application.Status = ApplicationStatus.Accepted;
					statusText = "được duyệt";
					break;
				case "reject":
					application.Status = ApplicationStatus.Rejected;
					statusText = "bị từ chối";
					break;
			}

			_context.Update(application);
			await _context.SaveChangesAsync();

			// Gửi email cho ứng viên
			await _emailService.SendEmailAsync(
				application.User.Email,
				"Kết quả ứng tuyển tại FPT",
				$"""
				<p>Chào {application.User.FullName},</p>
				<p>Hồ sơ ứng tuyển của bạn cho vị trí <strong>{application.Job.Title}</strong> đã <strong>{(status == "accept" ? "được duyệt" : "bị từ chối")}</strong>.</p>
				<p>Trân trọng,<br>Phòng Tuyển Dụng FPT</p>
				""");

			return RedirectToAction("Details", new { id = id });
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			var companyId = await GetCurrentUserCompanyIdAsync();

			var application = await _context.Applications
				.Include(a => a.Job)
				.FirstOrDefaultAsync(a => a.Id == id && a.Job.CompanyId == companyId);

			if (application == null)
			{
				return Forbid();
			}
			_context.Applications.Remove(application);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(CompanyApplicants));
		}
	}
}
