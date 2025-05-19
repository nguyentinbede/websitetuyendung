using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using TuyenDungFPT.Models;
using BC = BCrypt.Net.BCrypt;
using TuyenDungFPT.ViewModels;
using Microsoft.EntityFrameworkCore;
using TuyenDungFPT.Models.ViewModels;
using Newtonsoft.Json;
using SlugGenerator;
using Microsoft.Extensions.Hosting;

namespace TuyenDungFPT.Controllers
{
    public class HomeController : Controller
    {
		private readonly TuyenDungFPTDbContext _context;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly EmailService _emailService;
		public HomeController(TuyenDungFPTDbContext context, IHttpContextAccessor httpContextAccessor, EmailService emailService)
		{
			_context = context;
			_httpContextAccessor = httpContextAccessor;
			_emailService = emailService;
		}
		public async Task<IActionResult> Index()
		{
			var jobs = await _context.Jobs
				.Include(j => j.Company)
				.Include(j => j.JobCategory)
				.ToListAsync();

			var jobViewModels = jobs.Select(job => new JobViewModel
			{
				Job = job,
				IsSaved = false // hoặc lấy từ DB
			}).ToList();

			var categories = await _context.JobCategories.ToListAsync();

			var homeVM = new HomeViewModel
			{
				Jobs = jobViewModels, // ✅ Chuẩn
				Categories = categories
			};
			return View(homeVM);
		}


		// GET: Index
		//public IActionResult Index()
		//{
		//	return View();
		//}
		// GET: Login
		[AllowAnonymous]
		public IActionResult Login(string? ReturnUrl)
		{
			if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
			{
				// Nếu đã đăng nhập thì chuyển đến trang chủ
				return LocalRedirect(ReturnUrl ?? "/");
			}
			else
			{
				// Nếu chưa đăng nhập thì chuyển đến trang đăng nhập
				ViewBag.LienKetChuyenTrang = ReturnUrl ?? "/";
				return View();
			}
		}
		// POST: Login
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Login([Bind] DangNhap dangNhap)
		{
			if (ModelState.IsValid)
			{
				var nguoiDung = _context.Users.Where(r => r.Email == dangNhap.Email).SingleOrDefault();
				if (nguoiDung == null || !BC.Verify(dangNhap.PasswordHash, nguoiDung.PasswordHash))
				{
					TempData["ThongBaoLoi"] = "Tài khoản không tồn tại trong hệ thống.";
					return View(dangNhap);
				}
				else
				{
					Console.WriteLine("Role người dùng là: " + nguoiDung.Role);
					var claims = new List<Claim>
						{
						new Claim("Id", nguoiDung.Id.ToString()),
						new Claim(ClaimTypes.Name, nguoiDung.Email),
						new Claim("FullName", nguoiDung.FullName),new Claim(ClaimTypes.Role, 
							nguoiDung.Role == UserRole.Admin ? "Admin" : 
							nguoiDung.Role == UserRole.Recruiter ? "Recruiter" : "Applicant"),
						new Claim(ClaimTypes.NameIdentifier, nguoiDung.Id.ToString())

						};
					var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
					var authProperties = new AuthenticationProperties
					{
						IsPersistent = dangNhap.DuyTriDangNhap
					};
					// Đăng nhập hệ thống
					await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
					new ClaimsPrincipal(claimsIdentity),
					authProperties);
					return LocalRedirect(
						dangNhap.LienKetChuyenTrang
						?? (nguoiDung.Role == UserRole.Admin ? "/Admin"
							: nguoiDung.Role == UserRole.Recruiter ? "/Recruiter"
							: "/Applicant"));
				}
			}
			return View(dangNhap);
		}
		// GET: DangXuat
		public async Task<IActionResult> Logout()
		{
			await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
			return RedirectToAction("Index", "Home", new { Area = "" });
		}
		// GET: Forbidden
		public IActionResult Forbidden()
		{
			return View();
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		[Authorize]


		[HttpGet]
		public async Task<IActionResult> Profile()
		{
			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
				return Unauthorized();

			var user = await _context.Users
				.Include(u => u.Resumes)
				.FirstOrDefaultAsync(u => u.Id == userId);
			if (user == null) return NotFound();

			var viewModel = new ProfileEditViewModel
			{
				Id = user.Id,
				FullName = user.FullName,
				FullName_khongdau = user.FullName_khongdau,
				Phone = user.Phone,
				Avata = user.Avata,
				Resumes = user.Resumes?.ToList() ?? new List<Resume>()
			};

			return View(viewModel);
		}

		[HttpPost]
		public async Task<IActionResult> Profile(ProfileEditViewModel model)
		{
			if (!ModelState.IsValid) return View(model);

			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
				return Unauthorized();

			var user = await _context.Users.FindAsync(userId);
			if (user == null) return NotFound();

			// Cập nhật thông tin cơ bản
			user.FullName = model.FullName;
			user.FullName_khongdau = model.FullName_khongdau;
			user.FullName_khongdau = string.IsNullOrWhiteSpace(model.FullName_khongdau)
						? model.FullName.GenerateSlug()
						: model.FullName_khongdau;
			user.Phone = model.Phone;
			if (string.IsNullOrEmpty(model.FullName_khongdau) || model.FullName_khongdau.Trim() == "")
			{
				model.FullName_khongdau = model.FullName.GenerateSlug();
			}
			// ✅ Cập nhật mật khẩu nếu có nhập mới
			if (!string.IsNullOrWhiteSpace(model.PasswordHash) && !string.IsNullOrWhiteSpace(model.XacNhanMatKhau))
			{
				if (model.PasswordHash == model.XacNhanMatKhau)
				{
					user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.PasswordHash);
				}
				else
				{
					ModelState.AddModelError("XacNhanMatKhau", "Mật khẩu xác nhận không khớp.");
					return View(model);
				}
			}

			// ✅ Xử lý ảnh đại diện nếu có
			if (model.DataAvata != null)
			{
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
				if (!Directory.Exists(uploadsFolder))
					Directory.CreateDirectory(uploadsFolder);

				var fileName = Path.GetFileName(model.DataAvata.FileName);
				var filePath = Path.Combine(uploadsFolder, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await model.DataAvata.CopyToAsync(stream);
				}

				user.Avata = fileName;
			}

			await _context.SaveChangesAsync();

			TempData["Success"] = "Cập nhật thông tin thành công!";
			return RedirectToAction("Profile");
		}

		[HttpPost]
		public async Task<IActionResult> UploadResume(IFormFile DataFilePath)
		{
			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
				return Unauthorized();

			// Kiểm tra số lượng CV đã tải lên
			var resumeCount = await _context.Resumes.CountAsync(r => r.UserId == userId);
			if (resumeCount >= 5)
			{
				TempData["Error"] = "Bạn chỉ có thể tải lên tối đa 5 CV.";
				return RedirectToAction("Profile");
			}

			if (DataFilePath != null && DataFilePath.Length > 0)
			{
				var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
				if (!Directory.Exists(uploadsFolder))
					Directory.CreateDirectory(uploadsFolder);

				var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(DataFilePath.FileName)}";
				var filePath = Path.Combine(uploadsFolder, fileName);

				using (var stream = new FileStream(filePath, FileMode.Create))
				{
					await DataFilePath.CopyToAsync(stream);
				}

				var newResume = new Resume
				{
					UserId = userId,
					FilePath = fileName,
					CreatedAt = DateTime.UtcNow
				};

				_context.Resumes.Add(newResume);
				await _context.SaveChangesAsync();

				TempData["Success"] = "Tải lên CV thành công!";
			}
			else
			{
				TempData["Error"] = "Vui lòng chọn một tệp hợp lệ.";
			}

			return RedirectToAction("Profile");
		}


		[HttpPost]
		public async Task<IActionResult> DeleteResume(int id)
		{
			var resume = await _context.Resumes.FindAsync(id);
			if (resume == null) return NotFound();

			if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId) || resume.UserId != userId)
				return Unauthorized();

			var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/cv", resume.FilePath ?? "");
			if (System.IO.File.Exists(filePath))
			{
				System.IO.File.Delete(filePath);
			}

			_context.Resumes.Remove(resume);
			await _context.SaveChangesAsync();

			TempData["Success"] = "Đã xóa CV thành công!";
			return RedirectToAction("Profile");
		}
		// GET: Register
		[AllowAnonymous]
		public IActionResult Register(string? successMessage)
		{
			if (!string.IsNullOrEmpty(successMessage))
				TempData["ThongBao"] = successMessage;
			return View();
		}

		// POST: Register
		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> Register(RegisterViewModel model)
		{
			if (ModelState.IsValid)
			{
				var check = _context.Users.SingleOrDefault(u => u.Email == model.Email);
				if (check != null)
				{
					ModelState.AddModelError("", "Email đã tồn tại.");
					return View(model);
				}

				// ✅ Tạo mã OTP
				var otp = new Random().Next(100000, 999999).ToString();

				// ✅ Gửi OTP qua email
				string body = $"<p>Mã xác nhận đăng ký tài khoản của bạn là: <strong>{otp}</strong></p>";
				await _emailService.SendEmailAsync(model.Email, "Xác minh đăng ký", body);

				// ✅ Lưu thông tin và OTP vào TempData
				TempData["OTP"] = otp;
				TempData["RegisterInfo"] = JsonConvert.SerializeObject(model);
				TempData.Keep();

				return RedirectToAction("VerifyOTP");
			}

			return View(model);
		}
		[HttpGet]
		[AllowAnonymous]
		public IActionResult VerifyOTP()
		{
			return View();
		}

		[HttpPost]
		[AllowAnonymous]
		public async Task<IActionResult> VerifyOTP(string otpInput)
		{
			var otp = TempData["OTP"] as string;
			var registerJson = TempData["RegisterInfo"] as string;

			if (otpInput == otp && registerJson != null)
			{
				var model = JsonConvert.DeserializeObject<RegisterViewModel>(registerJson);

				var user = new User
				{
					FullName = model.FullName,
					Email = model.Email,
					PasswordHash = BC.HashPassword(model.Password),
					Role = UserRole.Applicant,
					EmailConfirmed = true
				};

				_context.Users.Add(user);
				await _context.SaveChangesAsync();

				TempData["ThongBao"] = "Đăng ký thành công. Bạn có thể đăng nhập.";
				return RedirectToAction("Login", "Home");
			}

			ModelState.AddModelError("", "Mã OTP không hợp lệ.");
			return View();
		}
		public IActionResult GioiThieu()
		{
			return View();
		}
		public IActionResult LienHe()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> LienHe(ContactFormViewModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			string body = $@"
            <h4>Liên hệ từ {model.Name}</h4>
            <p><strong>Email:</strong> {model.Email}</p>
            <p><strong>Chủ đề:</strong> {model.Subject}</p>
            <p><strong>Nội dung:</strong></p>
            <p>{model.Message}</p>";

			await _emailService.SendEmailAsync("cuoivcvl@gmail.com", "Liên hệ mới từ website", body);

			ViewBag.Message = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";

			return View();
		}
	}
}
