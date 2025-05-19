using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models;

namespace TuyenDungFPT.Controllers
{
    public class BaiVietController : Controller
    {
        private readonly TuyenDungFPTDbContext _context;
		private readonly HttpContext _httpContext = new HttpContextAccessor().HttpContext;

		public BaiVietController(TuyenDungFPTDbContext context)
        {
            _context = context;
        }

		// GET: Index
		public IActionResult Index(int? trang)
		{
			var danhSach = LayDanhSachBaiViet(trang ?? 1);
			if (danhSach.BaiViet.Count == 0)
				return NotFound();
			else
				return View(danhSach);
		}

		private PhanTrangBaiViet LayDanhSachBaiViet(int trangHienTai)
		{
			int maxRows = 12;

			PhanTrangBaiViet phanTrang = new PhanTrangBaiViet();
			phanTrang.BaiViet = _context.BaiViet
			.Include(s => s.User)
			.Include(s => s.ChuDe)
			.Where(r => r.KiemDuyet == true && r.HienThi == true)
			.OrderByDescending(r => r.NgayDang)
			.Skip((trangHienTai - 1) * maxRows)
			.Take(maxRows).ToList();

			decimal tongSoTrang = Convert.ToDecimal(_context.BaiViet.Count()) / Convert.ToDecimal(maxRows);
			phanTrang.TongSoTrang = (int)Math.Ceiling(tongSoTrang);
			phanTrang.TrangHienTai = trangHienTai;

			return phanTrang;
		}

		// GET: ChuDe
		public IActionResult ChuDe(string tenChuDe, int? trang)
		{
			var danhSachPhanLoai = LayDanhSachBaiVietTheoChuDe(tenChuDe, trang ?? 1);
			if (danhSachPhanLoai.BaiViet.Count == 0)
				return NotFound();
			else
				return View(danhSachPhanLoai);
		}

		private PhanTrangBaiViet LayDanhSachBaiVietTheoChuDe(string tenChuDe, int trangHienTai)
		{
			int maxRows = 12;

			var baiVietTheoChuDe = _context.BaiViet
			.Include(s => s.User)
			.Include(s => s.ChuDe)
			.Where(r => r.KiemDuyet == true && r.HienThi == true && r.ChuDe.TenChuDeKhongDau == tenChuDe);

			PhanTrangBaiViet phanTrang = new PhanTrangBaiViet();
			phanTrang.BaiViet = baiVietTheoChuDe.OrderByDescending(r => r.NgayDang)
			.Skip((trangHienTai - 1) * maxRows)
			.Take(maxRows).ToList();

			decimal tongSoTrang = Convert.ToDecimal(baiVietTheoChuDe.Count()) / Convert.ToDecimal(maxRows);
			phanTrang.TongSoTrang = (int)Math.Ceiling(tongSoTrang);
			phanTrang.TrangHienTai = trangHienTai;

			return phanTrang;
		}

		// GET: ChiTiet
		public IActionResult ChiTiet(string tenChuDe, string tieuDe)
		{
			var baiViet = _context.BaiViet
			.Include(s => s.User)
			.Include(s => s.ChuDe)
			.Include(s => s.BinhLuanBaiViet)
				.ThenInclude(b => b.User)
			.Where(r => r.KiemDuyet == true && r.HienThi == true && r.TieuDeKhongDau == tieuDe).SingleOrDefault();
			if (baiViet == null)
				return NotFound();
			else
			{
				// Xử lý tăng lượt xem
				string _sessionKey = "DaXemBaiViet_" + baiViet.Id;

				if (string.IsNullOrEmpty(_httpContext.Session.GetString(_sessionKey)))
				{
					baiViet.LuotXem = baiViet.LuotXem + 1;
					_context.Update(baiViet);
					_context.SaveChanges();
					_httpContext.Session.SetString(_sessionKey, "1");
				}

				// Lấy bài viết cùng chuyên mục
				var baiVietCungChuyenMuc = _context.BaiViet
				.Include(s => s.User)
			   .Include(s => s.ChuDe)
			   .Where(r => r.KiemDuyet == true && r.HienThi == true && r.ChuDeId == baiViet.ChuDeId && r.Id != baiViet.Id)
				.OrderByDescending(r => r.NgayDang)
			   .Take(4);
				ViewData["BaiVietCungChuyenMuc"] = baiVietCungChuyenMuc;

				return View(baiViet);
			}
		}
		[HttpPost]
		public async Task<IActionResult> DangBinhLuan(int baiVietId, string noiDung)
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

			var binhLuan = new BinhLuanBaiViet
			{
				BaiVietId = baiVietId,
				NoiDungBinhLuan = noiDung,
				UserId = userId,
				NgayDang = DateTime.Now,
				KiemDuyet = false,
				LuotXem = 0
			};

			_context.BinhLuanBaiViet.Add(binhLuan);
			await _context.SaveChangesAsync();

			var baiViet = await _context.BaiViet
				.Include(b => b.ChuDe)
				.FirstOrDefaultAsync(b => b.Id == baiVietId);

			if (baiViet == null)
			{
				return NotFound();
			}

			return RedirectToAction("ChiTiet", new { tenChuDe = baiViet.ChuDe.TenChuDeKhongDau, tieuDe = baiViet.TieuDeKhongDau });
		}




	}
}
