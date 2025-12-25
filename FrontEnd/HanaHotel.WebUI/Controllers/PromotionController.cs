using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using HanaHotel.DataAccessLayer.Concrete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HanaHotel.WebUI.DTOs.PromotionDetailDTO;
using HanaHotel.WebUI.DTOs.PromotionDTO;

namespace HanaHotel.WebUI.Controllers
{
    public class PromotionController : Controller
    {
        private readonly IPromotionService _promotionService;
        private readonly DataContext _context;

        public PromotionController(IPromotionService promotionService, DataContext context)
        {
            _promotionService = promotionService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var promotions = await _context.Promotions
            .Include(p => p.PromotionDetails)
            .ThenInclude(d => d.Room)
            .ToListAsync();

            var model = promotions.Select(p => new PromotionListDto
            {
                PromotionId = p.PromotionID,
                PromotionName = p.PromotionName,
                DiscountAmount = p.DiscountAmount,
                StartDate = p.StartDate,
                EndDate = p.EndDate,
                Details = p.PromotionDetails.Select(d => new PromotionDetailListDto
                {
                    RoomId = d.RoomId,
                    RoomName = d.Room.RoomName, // lấy tên phòng
                    DiscountPercent = d.DiscountPercent
                }).ToList()
            }).ToList();

            return View(model);
        }

        // ======================= CREATE =======================
        // GET: hiển thị form tạo khuyến mãi
        public async Task<IActionResult> Create()
        {
            // Tạo SelectList để hiển thị phòng
            var rooms = await _context.Rooms
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),        // Id phòng dùng làm value
                    Text = $"{r.RoomName} ({r.Price:C})" // Tên phòng + giá hiển thị
                })
                .ToListAsync();

            ViewBag.Rooms = rooms;

            return View(new PromotionCreateDto
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7)
            });
        }

        // POST: tạo khuyến mãi
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PromotionCreateDto dto, List<int> SelectedRoomIds)
        {
            if (!ModelState.IsValid)
            {
                // Load lại danh sách phòng nếu form invalid
                var rooms = await _context.Rooms
                    .Select(r => new SelectListItem
                    {
                        Value = r.Id.ToString(),
                        Text = $"{r.RoomName} ({r.Price:C})"
                    })
                    .ToListAsync();
                ViewBag.Rooms = rooms;
                return View(dto);
            }

            dto.Details = new List<PromotionDetailCreateDto>();
            foreach (var roomId in SelectedRoomIds)
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null || room.Price <= 0) continue;

                // Tính phần trăm giảm
                decimal discountPercent = Math.Round((dto.DiscountAmount / room.Price) * 100, 2);

                dto.Details.Add(new PromotionDetailCreateDto
                {
                    RoomId = roomId,
                    DiscountPercent = discountPercent
                });
            }

            await _promotionService.AddAsync(dto);
            return RedirectToAction(nameof(Index));
        }


        // ======================= EDIT =======================
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _promotionService.GetByIdAsync(id); // Lấy DtoLayer DTO
            if (dto == null) return NotFound();

            // Map sang WebUI DTO
            var webDto = new PromotionCreateDTO
            {
                PromotionID = dto.PromotionID,
                PromotionName = dto.PromotionName,
                DiscountAmount = dto.DiscountAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Details = dto.Details?.Select(d => new CreatePromotionDetailDTO
                {
                    RoomId = d.RoomId,
                    DiscountPercent = (byte)d.DiscountPercent
                }).ToList()
            };

            var rooms = await _context.Rooms.ToListAsync();
            ViewData["Rooms"] = rooms.Select(r => new SelectListItem
            {
                Value = r.Id.ToString(),
                Text = $"{r.RoomName} ({r.Price:C})"
            }).ToList();

            ViewBag.SelectedRooms = webDto.Details?.Select(d => d.RoomId).ToList() ?? new List<int>();

            return View(webDto);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PromotionCreateDTO dto, List<int> SelectedRoomIds)
        {
            if (!ModelState.IsValid)
            {
                var rooms = await _context.Rooms.ToListAsync();
                ViewData["Rooms"] = rooms.Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = $"{r.RoomName} ({r.Price:C})"
                }).ToList();

                ViewBag.SelectedRooms = SelectedRoomIds;
                return View(dto);
            }

            // Map WebUI DTO -> DtoLayer DTO
            var updateDto = new HanaHotel.DtoLayer.DTOs.PromotionDTO.PromotionCreateDto
            {
                PromotionID = dto.PromotionID,
                PromotionName = dto.PromotionName,
                DiscountAmount = dto.DiscountAmount,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Details = SelectedRoomIds.Select(roomId =>
                {
                    var room = _context.Rooms.Find(roomId);
                    return new HanaHotel.DtoLayer.DTOs.PromotionDTO.PromotionDetailCreateDto
                    {
                        RoomId = roomId,
                        DiscountPercent = room != null && room.Price > 0
                            ? Math.Round((dto.DiscountAmount / room.Price) * 100, 2)
                            : 0
                    };
                }).ToList()
            };

            await _promotionService.UpdateAsync(id, updateDto);
            return RedirectToAction(nameof(Index));
        }




        // ======================= DELETE =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
