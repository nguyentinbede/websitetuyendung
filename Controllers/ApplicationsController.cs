using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TuyenDungFPT.Models;

namespace TuyenDungFPT.Controllers
{
	[Authorize] // Bắt buộc đăng nhập mới được ứng tuyển
	public class ApplicationsController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;

		public ApplicationsController(TuyenDungFPTDbContext context)
		{
			_context = context;
		}
	
		public async Task<IActionResult> MyApplications()
		{
			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdStr, out int userId))
			{
				return RedirectToAction("Login", "Home");
			}

			var applications = await _context.Applications
				.Include(a => a.Job)
					.ThenInclude(j => j.Company)
				.Where(a => a.UserId == userId)
				.OrderByDescending(a => a.AppliedAt)
				.ToListAsync();

			return View(applications);
		}
	
		// Hiển thị form chọn CV để nộp
		[HttpGet]
		public async Task<IActionResult> Apply(int jobId)
		{
			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
				return Unauthorized();

			// Kiểm tra đã ứng tuyển chưa
			bool alreadyApplied = await _context.Applications
				.AnyAsync(a => a.JobId == jobId && a.UserId == userId);

			if (alreadyApplied)
			{
				TempData["Error"] = "Bạn đã ứng tuyển công việc này rồi.";
				return RedirectToAction("Details", "Jobs", new { id = jobId });
			}

			// Lấy danh sách CV của người dùng
			var resumes = await _context.Resumes
				.Where(r => r.UserId == userId)
				.ToListAsync();

			if (resumes.Count == 0)
			{
				TempData["Error"] = "Bạn cần tải lên ít nhất một CV để ứng tuyển.";
				return RedirectToAction("Profile", "Users");
			}

			// Lấy thông tin công việc để hiển thị
			var job = await _context.Jobs.FindAsync(jobId);

			// Truyền dữ liệu sang view qua ViewBag
			ViewBag.Job = job;
			ViewBag.Resumes = resumes;

			// Tạo instance Application để truyền vào view
			var application = new Application
			{
				JobId = jobId
			};

			// ✅ Trả về view với model
			return View(application);
		}


		// Xử lý khi người dùng chọn CV để nộp
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Apply(int jobId, int resumeId)
		{
			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
				return Unauthorized();

			// Kiểm tra đã nộp chưa
			bool alreadyApplied = await _context.Applications.AnyAsync(a => a.JobId == jobId && a.UserId == userId);
			if (alreadyApplied)
			{
				TempData["Error"] = "Bạn đã ứng tuyển công việc này rồi.";
				return RedirectToAction("Details", "Jobs", new { id = jobId });
			}

			// Kiểm tra CV thuộc về người dùng không
			var resume = await _context.Resumes.FirstOrDefaultAsync(r => r.Id == resumeId && r.UserId == userId);
			if (resume == null)
			{
				TempData["Error"] = "CV không hợp lệ.";
				return RedirectToAction("Apply", new { jobId });
			}

			// Tạo bản ghi ứng tuyển
			var application = new Application
			{
				JobId = jobId,
				UserId = userId,
				ResumeId = resumeId,
				Status = ApplicationStatus.Pending,
				AppliedAt = DateTime.UtcNow
			};

			_context.Applications.Add(application);
			await _context.SaveChangesAsync();

			// ✅ Thêm thông báo
			TempData["Success"] = "Ứng tuyển thành công!";

			// ✅ Chuyển về trang chủ (Home/Index)
			return RedirectToAction("Index", "Home");
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Withdraw(int id)
		{
			var application = await _context.Applications.FindAsync(id);
			if (application == null) return NotFound();

			var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (!int.TryParse(userIdStr, out int userId) || application.UserId != userId)
			{
				return Unauthorized();
			}

			_context.Applications.Remove(application);
			await _context.SaveChangesAsync();

			TempData["SuccessMessage"] = "Bạn đã rút đơn thành công.";
			return RedirectToAction("MyApplications");
		}

	}
}
