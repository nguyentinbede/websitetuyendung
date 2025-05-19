using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SlugGenerator;
using TuyenDungFPT.Models;
using BC = BCrypt.Net.BCrypt;

namespace TuyenDungFPT.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class UsersController : Controller
	{
		private readonly TuyenDungFPTDbContext _context;
		private readonly IWebHostEnvironment _hostEnvironment;

		public UsersController(TuyenDungFPTDbContext context, IWebHostEnvironment hostEnvironment)
		{
			_context = context;
			_hostEnvironment = hostEnvironment;
		}

		// GET: Users
		public async Task<IActionResult> Index()
		{
			var tuyenDungFPTDbContext = _context.Users.Include(u => u.Company);
			return View(await tuyenDungFPTDbContext.ToListAsync());
		}

		// GET: Users/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.Users
				.Include(u => u.Company)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// GET: Users/Create
		public IActionResult Create()
		{
			ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
			return View();
		}

		// POST: Users/Create
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("Id,CompanyId,FullName,Email,PasswordHash,XacNhanMatKhau,Phone,Avata,DataAvata,Role,CreatedAt")] User user)
		{
			if (!ModelState.IsValid)
			{
				foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
				{
					Console.WriteLine($"Lỗi ModelState: {error.ErrorMessage}");
				}
				return View(user);
			}

			string path = "";

			// Kiểm tra và upload file nếu có
			if (user.DataAvata != null)
			{
				string wwwRootPath = _hostEnvironment.WebRootPath;
				string folder = "/uploads/";
				string fileExtension = Path.GetExtension(user.DataAvata.FileName).ToLower();
				string fileName = $"{Guid.NewGuid()}{fileExtension}";

				path = fileName; // Lưu tên file vào DB
				string physicalPath = Path.Combine(wwwRootPath + folder, fileName);

				try
				{
					using (var fileStream = new FileStream(physicalPath, FileMode.Create))
					{
						await user.DataAvata.CopyToAsync(fileStream);
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine($"❌ Lỗi khi lưu file: {ex.Message}");
					ModelState.AddModelError("FileUpload", "Không thể lưu file. Vui lòng thử lại.");
					return View(user);
				}
			}

			// Cập nhật đường dẫn file vào database
			user.Avata = path;

			try
			{
				user.PasswordHash = BC.HashPassword(user.PasswordHash);
				user.XacNhanMatKhau = user.PasswordHash;
				if (string.IsNullOrEmpty(user.FullName_khongdau) || user.FullName_khongdau.Trim() == "")
				{
					user.FullName_khongdau = user.FullName.GenerateSlug();
				}
				user.XacNhanMatKhau = user.PasswordHash;

				_context.Add(user);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				Console.WriteLine($"❌ Lỗi khi lưu vào CSDL: {ex.Message}");
				ModelState.AddModelError("", "Lỗi khi lưu vào cơ sở dữ liệu.");
			}
			ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", user.CompanyId);
			return View(user);
		}

		// GET: Users/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", user.CompanyId);
			return View(new NguoiDung_ChinhSua(user));

		}

		// POST: Users/Edit/5
		// To protect from overposting attacks, enable the specific properties you want to bind to.
		// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,FullName,Email,PasswordHash,XacNhanMatKhau,Phone,Avata,DataAvata,Role,CreatedAt")] NguoiDung_ChinhSua user)
		{
			if (id != user.Id)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					var existingUser = await _context.Users.FindAsync(id);
					if (existingUser == null)
					{
						return NotFound();
					}

					string wwwRootPath = _hostEnvironment.WebRootPath;
					string folder = "uploads";
					string path = existingUser.Avata; // Mặc định giữ nguyên avatar cũ

					// Xử lý ảnh đại diện nếu có upload mới
					if (user.DataAvata != null && user.DataAvata.Length > 0)
					{
						// Xóa ảnh cũ nếu có
						if (!string.IsNullOrEmpty(existingUser.Avata))
						{
							string oldImagePath = Path.Combine(wwwRootPath, folder, existingUser.Avata);
							if (System.IO.File.Exists(oldImagePath))
							{
								System.IO.File.Delete(oldImagePath);
							}
						}

						// Tạo tên file mới
						string fileExtension = Path.GetExtension(user.DataAvata.FileName).ToLower();
						string fileName = $"{Guid.NewGuid()}{fileExtension}";

						path = fileName; // Cập nhật avatar mới
						string physicalPath = Path.Combine(wwwRootPath, folder, fileName);

						using (var fileStream = new FileStream(physicalPath, FileMode.Create))
						{
							await user.DataAvata.CopyToAsync(fileStream);
						}
					}

					// Cập nhật thông tin người dùng
					existingUser.FullName = user.FullName;
					existingUser.FullName_khongdau = string.IsNullOrWhiteSpace(user.FullName_khongdau)
						? user.FullName.GenerateSlug()
						: user.FullName_khongdau;
					existingUser.Email = user.Email;
					existingUser.Phone = user.Phone;
					existingUser.Role = user.Role;
					existingUser.CreatedAt = user.CreatedAt;
					existingUser.Avata = path; // Cập nhật avatar mới nếu có
					existingUser.CompanyId = user.CompanyId;

					// Cập nhật mật khẩu nếu có nhập mới
					if (!string.IsNullOrEmpty(user.PasswordHash))
					{
						existingUser.PasswordHash = BC.HashPassword(user.PasswordHash);
						existingUser.XacNhanMatKhau = user.PasswordHash;
					}
					if (string.IsNullOrEmpty(user.FullName_khongdau) || user.FullName_khongdau.Trim() == "")
					{
						user.FullName_khongdau = user.FullName.GenerateSlug();
					}
					_context.Update(existingUser);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!_context.Users.Any(e => e.Id == user.Id))
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
			ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", user.CompanyId);
			return View(user);
		}

		// GET: Users/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var user = await _context.Users
				.Include(u => u.Company)
				.FirstOrDefaultAsync(m => m.Id == id);
			if (user == null)
			{
				return NotFound();
			}

			return View(user);
		}

		// POST: Users/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}

			// Xóa các bản ghi liên quan trước
			var applications = await _context.Applications.Where(a => a.UserId == id).ToListAsync();
			_context.Applications.RemoveRange(applications);

			var savedJobs = await _context.SavedJobs.Where(sj => sj.UserId == id).ToListAsync();
			_context.SavedJobs.RemoveRange(savedJobs);

			var resumes = await _context.Resumes.Where(r => r.UserId == id).ToListAsync();
			_context.Resumes.RemoveRange(resumes);

			// Xóa hình ảnh nếu có
			if (!string.IsNullOrEmpty(user.Avata))
			{
				var imagePath = Path.Combine(_hostEnvironment.WebRootPath, "uploads", user.Avata);
				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

			// Cuối cùng xóa User
			_context.Users.Remove(user);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}

	}
}
