using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.RoleDTO;

namespace HanaHotel.WebUI.Controllers
{
    public class AdminRoleController : Controller
    {
        private readonly RoleManager<Role> _roleManager;
        private readonly IMapper _mapper;

        public AdminRoleController(RoleManager<Role> roleManager, IMapper mapper)
        {
            _roleManager = roleManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            return View(roles);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRole(CreateRoleDTO createRoleDTO)
        {
            if (ModelState.IsValid)
            {
                var role = _mapper.Map<Role>(createRoleDTO);
                var result = await _roleManager.CreateAsync(role);

                if (result.Succeeded)
                    return RedirectToAction("Index");

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(createRoleDTO);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                TempData["Error"] = "Không tìm thấy vai trò.";
                return RedirectToAction("Index");
            }

            var roleDTO = _mapper.Map<UpdateRoleDTO>(role);
            return View(roleDTO);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(UpdateRoleDTO updateRoleDTO)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(updateRoleDTO.Id.ToString());

                if (role == null)
                {
                    TempData["Error"] = "Không tìm thấy vai trò.";
                    return RedirectToAction("Index");
                }

                _mapper.Map(updateRoleDTO, role);
                var result = await _roleManager.UpdateAsync(role);

                if (result.Succeeded)
                    return RedirectToAction("Index");

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(updateRoleDTO);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());

            if (role == null)
            {
                TempData["Error"] = "Không tìm thấy vai trò.";
                return RedirectToAction("Index");
            }

            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
                TempData["Error"] = "Lỗi khi xóa vai trò. Vui lòng thử lại.";
            else
                TempData["Success"] = "Xóa vai trò thành công.";

            return RedirectToAction("Index");
        }
    }
}
