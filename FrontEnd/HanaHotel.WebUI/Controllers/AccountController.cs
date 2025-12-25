using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.UserDTO;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize] // only authenticated users can access
	public class AccountController : Controller
	{
		private readonly SignInManager<User> _signInManager;
		private readonly UserManager<User> _userManager;

		public AccountController(SignInManager<User> signInManager, UserManager<User> userManager)
		{
			_signInManager = signInManager;
			_userManager = userManager;
		}

		// Account info page
		[HttpGet]
		public async Task<IActionResult> Info()
		{
			// Prefer Identity principal instead of relying on Session
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				// If principal is missing or session stale, sign out to clear cookies and redirect to login
				await _signInManager.SignOutAsync();
				HttpContext.Session.Clear();
				return RedirectToAction("Index", "Login");
			}

			ViewBag.UserName = user.UserName;
			ViewBag.Email = user.Email;

			var roles = await _userManager.GetRolesAsync(user);
			ViewBag.Role = roles.FirstOrDefault() ?? HttpContext.Session.GetString("UserRole") ?? string.Empty;

			return View(user);
		}

		// Logout
		[HttpGet]
		public async Task<IActionResult> Logout()
		{
			await _signInManager.SignOutAsync();
			HttpContext.Session.Clear();
			return RedirectToAction("Index", "Default");
		}

		// GET: show edit form for currently logged-in user
		[HttpGet]
		public async Task<IActionResult> Edit()
		{
			var user = await _userManager.GetUserAsync(User);
			if (user == null)
			{
				await _signInManager.SignOutAsync();
				HttpContext.Session.Clear();
				return RedirectToAction("Index", "Login");
			}

			// Map User -> EditAccountDTO (align with current DTO)
			var model = new EditAccountDTO
			{
				Id = user.Id,
				Name = user.Name,
				DateOfBirth = user.DateOfBirth,
				Gender = user.Gender,
				Address = user.Address,
				Phone = user.PhoneNumber,
				Email = user.Email,
				UserName = user.UserName,
				RoleId = user.RoleId
			};

			return View(model);
		}

		// POST: save edits
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(EditAccountDTO model)
		{
			if (!ModelState.IsValid)
				return View(model);

			var user = await _userManager.FindByIdAsync(model.Id.ToString());
			if (user == null)
			{
				ModelState.AddModelError(string.Empty, "User not found.");
				return View(model);
			}

			// Map fields from DTO to User
			user.Name = model.Name;
			user.DateOfBirth = model.DateOfBirth;
			user.Gender = model.Gender;
			user.UserName = model.UserName;
			user.Email = model.Email;
			user.PhoneNumber = model.Phone;
			user.Address = model.Address;
			user.RoleId = model.RoleId;

			if (!string.IsNullOrEmpty(model.Password))
			{
				// Hash password and set PasswordHash
				user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
			}

			var result = await _userManager.UpdateAsync(user);
			if (!result.Succeeded)
			{
				foreach (var err in result.Errors)
					ModelState.AddModelError(string.Empty, err.Description);
				return View(model);
			}

			// Update session username/role from the source of truth (Identity)
			HttpContext.Session.SetString("UserName", user.UserName ?? string.Empty);
			var roles = await _userManager.GetRolesAsync(user);
			if (roles != null && roles.Any())
			{
				HttpContext.Session.SetString("UserRole", roles.First());
			}

			return RedirectToAction("Info");
		}
	}
}
