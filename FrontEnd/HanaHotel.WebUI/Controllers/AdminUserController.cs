using AutoMapper;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.UserDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminUserController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IMapper _mapper;

        public AdminUserController(UserManager<User> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> UpdateUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return RedirectToAction("Index");

            var dto = new UpdateUserDTO
            {
                Id = user.Id,
                Name = user.Name,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Address = user.Address,
                Phone = user.PhoneNumber,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName,
                RoleId = user.RoleId
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO updateUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return View(updateUserDTO);
            }

            var user = await _userManager.FindByIdAsync(updateUserDTO.Id.ToString());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Không tìm thấy người dùng.");
                return View(updateUserDTO);
            }

            user.Name = updateUserDTO.Name;
            user.DateOfBirth = updateUserDTO.DateOfBirth;
            user.Gender = updateUserDTO.Gender;
            user.Address = updateUserDTO.Address;
            user.PhoneNumber = updateUserDTO.Phone;
            user.Email = updateUserDTO.Email;
            user.UserName = updateUserDTO.UserName;
            user.RoleId = updateUserDTO.RoleId;

            if (!string.IsNullOrEmpty(updateUserDTO.Password))
            {
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, updateUserDTO.Password);
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError(string.Empty, err.Description);

                return View(updateUserDTO);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction("Index", "AdminUser");
        }
    }
}
