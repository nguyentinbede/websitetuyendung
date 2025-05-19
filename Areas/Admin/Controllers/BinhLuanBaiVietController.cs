using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models;
using Microsoft.AspNetCore.Authorization;

namespace TuyenDungFPT.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class BinhLuanBaiVietController : Controller
    {
        private readonly TuyenDungFPTDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public BinhLuanBaiVietController(TuyenDungFPTDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Admin/BinhLuanBaiViet
        public async Task<IActionResult> Index()
        {
            var tuyenDungFPTDbContext = _context.BinhLuanBaiViet.Include(b => b.BaiViet).Include(b => b.User).OrderByDescending(r => r.NgayDang);
            return View(await tuyenDungFPTDbContext.ToListAsync());
        }

        // GET: Admin/BinhLuanBaiViet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var binhLuanBaiViet = await _context.BinhLuanBaiViet
                .Include(b => b.BaiViet)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (binhLuanBaiViet == null)
            {
                return NotFound();
            }

            return View(binhLuanBaiViet);
        }

        // GET: Admin/BinhLuanBaiViet/Create
        public IActionResult Create()
        {
            ViewData["BaiVietId"] = new SelectList(_context.BaiViet, "Id", "TieuDe");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/BinhLuanBaiViet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,BaiVietId,NoiDungBinhLuan")] BinhLuanBaiViet binhLuanBaiViet)
        {
            if (ModelState.IsValid)
            {
				int maNguoiDung = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "ID")?.Value);

				// Gán các giá trị mặc định
				binhLuanBaiViet.UserId = maNguoiDung;
				binhLuanBaiViet.NgayDang = DateTime.Now;
				binhLuanBaiViet.LuotXem = 0;
				binhLuanBaiViet.KiemDuyet = true;
				_context.Add(binhLuanBaiViet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["BaiVietId"] = new SelectList(_context.BaiViet, "Id", "TieuDe", binhLuanBaiViet.BaiVietId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", binhLuanBaiViet.UserId);
            return View(binhLuanBaiViet);
        }

        // GET: Admin/BinhLuanBaiViet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var binhLuanBaiViet = await _context.BinhLuanBaiViet.FindAsync(id);
            if (binhLuanBaiViet == null)
            {
                return NotFound();
            }
            ViewData["BaiVietId"] = new SelectList(_context.BaiViet, "Id", "TieuDe", binhLuanBaiViet.BaiVietId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", binhLuanBaiViet.UserId);
            return View(binhLuanBaiViet);
        }

        // POST: Admin/BinhLuanBaiViet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,BaiVietId,NoiDungBinhLuan,KiemDuyet")] BinhLuanBaiViet binhLuanBaiViet)
        {
            if (id != binhLuanBaiViet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
				{ // Bỏ qua không cập nhật
					var existing = await _context.BinhLuanBaiViet.FindAsync(binhLuanBaiViet.Id);
					if (existing == null) return NotFound();

					// Chỉ cập nhật các trường cho phép
					existing.BaiVietId = binhLuanBaiViet.BaiVietId;
					existing.NoiDungBinhLuan = binhLuanBaiViet.NoiDungBinhLuan;
					existing.KiemDuyet = binhLuanBaiViet.KiemDuyet;

					await _context.SaveChangesAsync();
					return RedirectToAction(nameof(Index));
				}
                catch (DbUpdateConcurrencyException)
                {
                    if (!BinhLuanBaiVietExists(binhLuanBaiViet.Id))
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
            ViewData["BaiVietId"] = new SelectList(_context.BaiViet, "Id", "TieuDe", binhLuanBaiViet.BaiVietId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", binhLuanBaiViet.UserId);
            return View(binhLuanBaiViet);
        }

        // GET: Admin/BinhLuanBaiViet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var binhLuanBaiViet = await _context.BinhLuanBaiViet
                .Include(b => b.BaiViet)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (binhLuanBaiViet == null)
            {
                return NotFound();
            }

            return View(binhLuanBaiViet);
        }

        // POST: Admin/BinhLuanBaiViet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var binhLuanBaiViet = await _context.BinhLuanBaiViet.FindAsync(id);
            if (binhLuanBaiViet != null)
            {
                _context.BinhLuanBaiViet.Remove(binhLuanBaiViet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BinhLuanBaiVietExists(int id)
        {
            return _context.BinhLuanBaiViet.Any(e => e.Id == id);
        }
    }
}
