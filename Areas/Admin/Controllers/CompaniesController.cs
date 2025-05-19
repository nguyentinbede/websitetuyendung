using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SlugGenerator;
using TuyenDungFPT.Models;
using SlugGenerator;

namespace TuyenDungFPT.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class CompaniesController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;
		private readonly IWebHostEnvironment _hostEnvironment;
		public CompaniesController(TuyenDungFPTDbContext context, IWebHostEnvironment hostEnvironment)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
		}

		// GET: Companies
		public async Task<IActionResult> Index()
		{
			return View(await _context.Companies.ToListAsync());
		}

		// GET: Companies/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var company = await _context.Companies
				.FirstOrDefaultAsync(m => m.Id == id);
			if (company == null)
			{
				return NotFound();
			}

			return View(company);
		}

		// GET: Companies/Create
		public IActionResult Create()
		{
			return View();
		}

		// POST: Companies/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,Name,Name_khongdau,Avata,DataAvata,Website,Industry,Location,Description")] Company company)
		{
			if (ModelState.IsValid)
			{
				if (string.IsNullOrWhiteSpace(company.Name_khongdau))
				{
					company.Name_khongdau = company.Name.GenerateSlug();
				}
				string path = "";

				// Nếu hình ảnh không bỏ trống thì upload
				if (company.DataAvata != null)
				{
					string wwwRootPath = _hostEnvironment.WebRootPath;
					string folder = "/uploads/";
					string fileExtension = Path.GetExtension(company.DataAvata.FileName).ToLower();
					string fileName = company.Name;
					string fileNameSluged = fileName.GenerateSlug();
					path = fileNameSluged + fileExtension;
					string physicalPath = Path.Combine(wwwRootPath + folder, fileNameSluged + fileExtension);
					using (var fileStream = new FileStream(physicalPath, FileMode.Create))
					{
						await company.DataAvata.CopyToAsync(fileStream);
					}
				}

				// Cập nhật đường dẫn vào CSDL
				company.Avata = path ?? null;
				_context.Add(company);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(company);
		}

		// GET: Companies/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var company = await _context.Companies.FindAsync(id);
			if (company == null)
			{
				return NotFound();
			}
			return View(company);
		}

		// POST: Companies/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Name_khongdau,Avata,DataAvata,Website,Industry,Location,Description")] Company company)
		{
			if (id != company.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var oldCompany = await _context.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
					if (oldCompany == null)
						return NotFound();
					if (string.IsNullOrWhiteSpace(company.Name_khongdau))
					{
						company.Name_khongdau = company.Name.GenerateSlug();
					}
					if (company.DataAvata != null)
					{
						// Xóa ảnh cũ
						if (!string.IsNullOrEmpty(oldCompany.Avata))
						{
							var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", oldCompany.Avata);
							if (System.IO.File.Exists(oldImagePath))
							{
								System.IO.File.Delete(oldImagePath);
							}
						}

						// Upload ảnh mới
						string wwwRootPath = _hostEnvironment.WebRootPath;
						string folder = "uploads";
						string fileExtension = Path.GetExtension(company.DataAvata.FileName).ToLower();
						string fileName = company.Name.GenerateSlug() + fileExtension;
						string physicalPath = Path.Combine(wwwRootPath, folder, fileName);
						using (var fileStream = new FileStream(physicalPath, FileMode.Create))
						{
							await company.DataAvata.CopyToAsync(fileStream);
						}

						company.Avata = fileName;
					}
					else
					{
						// Nếu không đổi ảnh, giữ nguyên ảnh cũ
						company.Avata = oldCompany.Avata;
					}

					_context.Update(company);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Companies.Any(e => e.Id == company.Id))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			return View(company);
		}

		// GET: Companies/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var company = await _context.Companies
				.FirstOrDefaultAsync(m => m.Id == id);
			if (company == null)
			{
				return NotFound();
			}

			return View(company);
		}

		// POST: Companies/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			using var transaction = await _context.Database.BeginTransactionAsync();

			try
			{
				// Lấy công ty cần xóa
				var company = await _context.Companies.FindAsync(id);
				if (company == null)
				{
					return NotFound();
				}

				// Lấy danh sách người dùng thuộc công ty
				var users = await _context.Users
					.Where(u => u.CompanyId == id)
					.ToListAsync();

				foreach (var user in users)
				{
					// Xóa Applications
					var applications = await _context.Applications
						.Where(a => a.UserId == user.Id)
						.ToListAsync();
					_context.Applications.RemoveRange(applications);

					// Xóa SavedJobs
					var savedJobs = await _context.SavedJobs
						.Where(sj => sj.UserId == user.Id)
						.ToListAsync();
					_context.SavedJobs.RemoveRange(savedJobs);

					// Xóa Resumes
					var resumes = await _context.Resumes
						.Where(r => r.UserId == user.Id)
						.ToListAsync();
					_context.Resumes.RemoveRange(resumes);
				}

				// Xóa Users thuộc công ty
				_context.Users.RemoveRange(users);

				// Xóa avatar nếu có
				if (!string.IsNullOrEmpty(company.Avata))
				{
					var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", company.Avata);
					if (System.IO.File.Exists(imagePath))
					{
						System.IO.File.Delete(imagePath);
					}
				}

				// Xóa công ty
				_context.Companies.Remove(company);

				// Lưu thay đổi
				await _context.SaveChangesAsync();
				await transaction.CommitAsync();

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				// Ghi log nếu cần
				ModelState.AddModelError("", "Lỗi khi xóa công ty: " + ex.Message);
				return View();
			}
		}
	}
}
