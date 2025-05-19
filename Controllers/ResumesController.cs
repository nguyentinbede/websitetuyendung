using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SlugGenerator;
using TuyenDungFPT.Models;

namespace TuyenDungFPT.Controllers
{
	public class ResumesController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;
		private readonly IWebHostEnvironment _hostEnvironment;

		public ResumesController(TuyenDungFPTDbContext context, IWebHostEnvironment hostEnvironment)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
		}

		// GET: Resumes
		public async Task<IActionResult> Index()
		{
			var tuyenDungFPTDbContext = _context.Resumes.Include(r => r.User);
			return View(await tuyenDungFPTDbContext.ToListAsync());
		}

		// GET: Resumes/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var resume = await _context.Resumes
				.Include(r => r.User)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (resume == null)
			{
				return NotFound();
			}

			return View(resume);
		}

		// GET: Resumes/Create
		public IActionResult Create()
		{
			ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
			return View();
		}

		// POST: Resumes/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,UserId,FilePath,DataFilePath,CreatedAt")] Resume resume)
		{
			if (!ModelState.IsValid) // Kiểm tra nếu ModelState không hợp lệ
			{
				foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
				{
					Console.WriteLine($"❌ Lỗi ModelState: {error.ErrorMessage}");
				}
				ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", resume.UserId);
				return View(resume); // Trả về View nếu có lỗi
			}

			string path = "";

			// Kiểm tra và upload file nếu có
			if (resume.DataFilePath != null)
			{
				string wwwRootPath = _hostEnvironment.WebRootPath;
				string folder = "/uploads/";
				string fileExtension = Path.GetExtension(resume.DataFilePath.FileName).ToLower();
				string fileName = $"{Guid.NewGuid()}{fileExtension}";

				path = fileName; // Lưu tên file vào DB
				string physicalPath = Path.Combine(wwwRootPath + folder, fileName);

				try
				{
					using (var fileStream = new FileStream(physicalPath, FileMode.Create))
					{
						await resume.DataFilePath.CopyToAsync(fileStream);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Lỗi khi lưu file: {ex.Message}");
					ModelState.AddModelError("FileUpload", "Không thể lưu file. Vui lòng thử lại.");
					ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", resume.UserId);
					return View(resume);
				}
			}

			// Cập nhật đường dẫn file vào database
			resume.FilePath = path;

			try
			{
				_context.Add(resume);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Lỗi khi lưu vào CSDL: {ex.Message}");
				ModelState.AddModelError("", "Lỗi khi lưu vào cơ sở dữ liệu.");
			}

			ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", resume.UserId);
			return View(resume);
		}

		// GET: Resumes/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var resume = await _context.Resumes.FindAsync(id);
			if (resume == null)
			{
				return NotFound();
			}
			ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", resume.UserId);
			return View(resume);
		}

		// POST: Resumes/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,FilePath,DataFilePath,CreatedAt")] Resume resume)
		{
			if (id != resume.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					string path = "";
					string wwwRootPath = _hostEnvironment.WebRootPath;
					string folder = "uploads";

					// Lấy ảnh cũ từ CSDL
					var existingResume = await _context.Resumes.AsNoTracking()
						.FirstOrDefaultAsync(r => r.Id == resume.Id);

					if (resume.DataFilePath != null)
					{
						// Xóa ảnh cũ nếu có
						if (!string.IsNullOrEmpty(existingResume?.FilePath))
						{
							string oldImagePath = Path.Combine(wwwRootPath, folder, existingResume.FilePath);
							if (System.IO.File.Exists(oldImagePath))
							{
								System.IO.File.Delete(oldImagePath);
							}
						}

						// Tạo tên file mới
						string fileExtension = Path.GetExtension(resume.DataFilePath.FileName).ToLower();
						string fileName = $"{Guid.NewGuid()}{fileExtension}";
						path = fileName;

						// Lưu file mới vào thư mục uploads
						string physicalPath = Path.Combine(wwwRootPath, folder, fileName);
						using (var fileStream = new FileStream(physicalPath, FileMode.Create))
						{
							await resume.DataFilePath.CopyToAsync(fileStream);
						}
					}

					_context.Update(resume);

					if (string.IsNullOrEmpty(path))
					{
						// Không thay đổi ảnh, giữ nguyên đường dẫn cũ
						_context.Entry(resume).Property(x => x.FilePath).IsModified = false;
					}
					else
					{
						// Cập nhật đường dẫn ảnh mới
						resume.FilePath = path;
					}

					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ResumeExists(resume.Id))
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

			ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", resume.UserId);
			return View(resume);
		}

		// GET: Resumes/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var resume = await _context.Resumes
				.Include(r => r.User)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (resume == null)
			{
				return NotFound();
			}

			return View(resume);
		}

		// POST: Resumes/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var resume = await _context.Resumes.FindAsync(id);
			if (resume != null)
			{
				// Xóa hình ảnh (nếu có)
				// Xác định đường dẫn file cần xóa
				var filePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", resume.FilePath);

				// Kiểm tra nếu file tồn tại thì xóa
				if (System.IO.File.Exists(filePath))
				{
					System.IO.File.Delete(filePath);
				}


				_context.Resumes.Remove(resume);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ResumeExists(int id)
		{
			return _context.Resumes.Any(e => e.Id == id);
		}
	}
}
