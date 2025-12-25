using System;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.LoginDTO;
using Microsoft.Extensions.Logging;

namespace HanaHotel.WebUI.Controllers
{
	[AllowAnonymous]
	public class LoginController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;
		private readonly ILogger<LoginController> _logger;

		public LoginController(SignInManager<User> signInManager, UserManager<User> userManager, ILogger<LoginController> logger)
		{
			_signInManager = signInManager;
			_userManager = userManager;
			_logger = logger;
		}

		[HttpGet]
		public IActionResult Index() => View();

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Index(LoginUserDTO loginUserDTO)
		{
			if (!ModelState.IsValid)
				return View(loginUserDTO);

			var user = await _userManager.FindByNameAsync(loginUserDTO.UserName);
			if (user == null)
			{
				ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
				return View(loginUserDTO);
			}

			var result = await _signInManager.PasswordSignInAsync(user, loginUserDTO.Password, false, false);
			if (!result.Succeeded)
			{
				// provide detailed feedback (locked/out/not allowed/2fa)
				if (result.IsLockedOut)
				{
					_logger.LogWarning("User {User} locked out", user.UserName);
					ModelState.AddModelError("", "Tài khoản bị khoá.");
				}
				else if (result.IsNotAllowed)
				{
					_logger.LogWarning("User {User} not allowed to sign in", user.UserName);
					ModelState.AddModelError("", "Tài khoản chưa được phép đăng nhập.");
				}
				else if (result.RequiresTwoFactor)
				{
					ModelState.AddModelError("", "Yêu cầu xác thực hai yếu tố.");
				}
				else
				{
					ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không chính xác.");
				}
				return View(loginUserDTO);
			}

			// Signed in successfully -> get roles from store (Identity) for fallback
			var roles = await _userManager.GetRolesAsync(user);
			_logger.LogInformation("User {User} roles: {Roles}", user.UserName, roles != null ? string.Join(",", roles) : "(none)");

			// Save session info
			HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);

			// Determine role: prefer explicit RoleId property on User (if exists), otherwise fallback to Identity roles.
			string determinedRole = null;

			try
			{
				var prop = user.GetType().GetProperty("RoleId");
				if (prop != null)
				{
					var val = prop.GetValue(user);
					if (val != null && int.TryParse(val.ToString(), out var roleId))
					{
						// TODO: Điều chỉnh mapping RoleId -> role name theo DB của bạn.
						// Ví dụ: 1 = Admin, 2 = Customer
						if (roleId == 1)
							determinedRole = "Admin";
						else
							determinedRole = "Customer";
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogDebug(ex, "Unable to read RoleId from user via reflection.");
			}

			// Fallback to Identity roles if RoleId không dùng được
			if (string.IsNullOrEmpty(determinedRole) && roles != null && roles.Any())
			{
				determinedRole = roles.First();
			}

			if (string.IsNullOrEmpty(determinedRole))
				determinedRole = "Customer"; // default

			HttpContext.Session.SetString("UserRole", determinedRole);

			// Redirect based on determined role
			if (string.Equals(determinedRole, "Admin", StringComparison.OrdinalIgnoreCase))
				return RedirectToAction("Index", "AdminDashboard");

			return RedirectToAction("Index", "Default");
		}
	}
}
