using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models;
using Microsoft.AspNetCore.Authorization;
using SlugGenerator;

namespace TuyenDungFPT.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = "Admin")]
	public class BaiVietController : Controller
    {
        private readonly TuyenDungFPTDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public BaiVietController(TuyenDungFPTDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
			_httpContextAccessor = httpContextAccessor;
		}

        // GET: Admin/BaiViet
        public async Task<IActionResult> Index()
        {
            var tuyenDungFPTDbContext = _context.BaiViet.Include(b => b.ChuDe).Include(b => b.User).OrderByDescending(r => r.NgayDang);
            return View(await tuyenDungFPTDbContext.ToListAsync());
        }

        // GET: Admin/BaiViet/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViet
                .Include(b => b.ChuDe)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiViet == null)
            {
                return NotFound();
            }

            return View(baiViet);
        }

        // GET: Admin/BaiViet/Create
        public IActionResult Create()
        {
            ViewData["ChuDeId"] = new SelectList(_context.ChuDe, "Id", "TenChuDe");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email");
            return View();
        }

        // POST: Admin/BaiViet/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ChuDeId,TieuDe,TieuDeKhongDau,TomTat,NoiDung,HienThi")] BaiViet baiViet)
        {
            if (ModelState.IsValid)
            {
				int maNguoiDung = Convert.ToInt32(_httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id")?.Value);

				// Gán các giá trị mặc định
				baiViet.UserId = maNguoiDung;
				baiViet.NgayDang = DateTime.Now;
				baiViet.LuotXem = 0;
				baiViet.KiemDuyet = true;

				// Nếu tiêu đề không dấu bỏ trống thì tự động xử lý
				if (string.IsNullOrWhiteSpace(baiViet.TieuDeKhongDau))
				{
					baiViet.TieuDeKhongDau = baiViet.TieuDe.GenerateSlug();
				}

				_context.Add(baiViet);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ChuDeId"] = new SelectList(_context.ChuDe, "Id", "TenChuDe", baiViet.ChuDeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", baiViet.UserId);
            return View(baiViet);
        }

        // GET: Admin/BaiViet/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViet.FindAsync(id);
            if (baiViet == null)
            {
                return NotFound();
            }
            ViewData["ChuDeId"] = new SelectList(_context.ChuDe, "Id", "TenChuDe", baiViet.ChuDeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", baiViet.UserId);
            return View(baiViet);
        }

        // POST: Admin/BaiViet/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ChuDeId,TieuDe,TieuDeKhongDau,TomTat,NoiDung,KiemDuyet,HienThi")] BaiViet baiViet)
        {
            if (id != baiViet.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
					// Nếu tiêu đề không dấu bỏ trống thì tự động xử lý
					if (string.IsNullOrWhiteSpace(baiViet.TieuDeKhongDau))
					{
						baiViet.TieuDeKhongDau = baiViet.TieuDe.GenerateSlug();
					}
					_context.Update(baiViet);
					// Bỏ qua không cập nhật
					_context.Entry(baiViet).Property(x => x.UserId).IsModified = false;
					_context.Entry(baiViet).Property(x => x.NgayDang).IsModified = false;
					_context.Entry(baiViet).Property(x => x.LuotXem).IsModified = false;

					await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BaiVietExists(baiViet.Id))
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
            ViewData["ChuDeId"] = new SelectList(_context.ChuDe, "Id", "TenChuDe", baiViet.ChuDeId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Email", baiViet.UserId);
            return View(baiViet);
        }

        // GET: Admin/BaiViet/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var baiViet = await _context.BaiViet
                .Include(b => b.ChuDe)
                .Include(b => b.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (baiViet == null)
            {
                return NotFound();
            }

            return View(baiViet);
        }

        // POST: Admin/BaiViet/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var baiViet = await _context.BaiViet.FindAsync(id);
            if (baiViet != null)
            {
                _context.BaiViet.Remove(baiViet);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BaiVietExists(int id)
        {
            return _context.BaiViet.Any(e => e.Id == id);
        }
    }
}
