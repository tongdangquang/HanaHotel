using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.Models.Role;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace HanaHotel.WebUI.Controllers
{
    public class AdminRoleAssignController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;

        public AdminRoleAssignController(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var values = await _userManager.Users.ToListAsync();
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                return RedirectToAction("Error404", "ErrorPage");
            }

            TempData["user_id"] = user.Id;

            var roles = await _roleManager.Roles.ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var roleAssignViewModels = roles.Select(role => new RoleAssignViewModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                RoleExist = userRoles.Contains(role.Name)
            }).ToList();

            return View(roleAssignViewModels);
        }

        [HttpPost]
        public async Task<IActionResult> EditRole(List<RoleAssignViewModel> roleAssignViewModels)
        {
            if (roleAssignViewModels == null)
                return RedirectToAction("Index");

            var userIdObj = TempData.Peek("user_id");
            if (userIdObj == null || !int.TryParse(userIdObj.ToString(), out var user_id))
                return RedirectToAction("Index");

            var user = await _userManager.FindByIdAsync(user_id.ToString());
            if (user == null)
                return RedirectToAction("Error404", "ErrorPage");

            var currentRoles = (await _userManager.GetRolesAsync(user)).ToHashSet();

            foreach (var item in roleAssignViewModels)
            {
                if (string.IsNullOrWhiteSpace(item.RoleName))
                    continue;

                var shouldExist = item.RoleExist;
                var existsNow = currentRoles.Contains(item.RoleName);

                if (shouldExist && !existsNow)
                {
                    await _userManager.AddToRoleAsync(user, item.RoleName);
                }
                else if (!shouldExist && existsNow)
                {
                    await _userManager.RemoveFromRoleAsync(user, item.RoleName);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
