using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.RegisterDTO;

namespace HanaHotel.WebUI.Controllers
{
    [AllowAnonymous]
    public class RegisterController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ILogger<RegisterController> _logger;

        public RegisterController(UserManager<User> userManager, RoleManager<Role> roleManager, ILogger<RegisterController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(CreateUserDTO createUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(createUserDTO);
            }

            var appUser = new User
            {
                Name = createUserDTO.Name,
                DateOfBirth = createUserDTO.DateOfBirth,
                Gender = createUserDTO.Gender,
                Address = createUserDTO.Address,
                PhoneNumber = createUserDTO.Phone, // prefer Identity's PhoneNumber
                Email = createUserDTO.Email,
                UserName = createUserDTO.UserName
            };

            var result = await _userManager.CreateAsync(appUser, createUserDTO.Password);

            if (result.Succeeded)
            {
                var customerRoleName = "Customer";
                if (!await _roleManager.RoleExistsAsync(customerRoleName))
                {
                    var createRoleResult = await _roleManager.CreateAsync(new Role { Name = customerRoleName });
                    if (!createRoleResult.Succeeded)
                    {
                        foreach (var error in createRoleResult.Errors)
                            ModelState.AddModelError(string.Empty, error.Description);

                        return View(createUserDTO);
                    }
                }

                var addToRoleResult = await _userManager.AddToRoleAsync(appUser, customerRoleName);
                if (!addToRoleResult.Succeeded)
                {
                    foreach (var error in addToRoleResult.Errors)
                        ModelState.AddModelError(string.Empty, error.Description);

                    return View(createUserDTO);
                }

                _logger.LogInformation("User {User} created and assigned to role {Role}", appUser.UserName, customerRoleName);
                return RedirectToAction("Index", "Login");
            }

            // Log and surface Identity errors so you know why CreateAsync failed
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Register failed for {User}: {Code} - {Description}", createUserDTO.UserName, error.Code, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(createUserDTO);
        }
    }
}
