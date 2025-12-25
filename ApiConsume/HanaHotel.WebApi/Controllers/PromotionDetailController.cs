using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HanaHotel.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionDetailController : ControllerBase
    {
        private readonly DataContext _context;

        public PromotionDetailController(DataContext context)
        {
            _context = context;
        }

        // GET: api/PromotionDetail
        [HttpGet]
        public async Task<ActionResult<List<PromotionDetailListDto>>> GetAll()
        {
            var details = await _context.PromotionDetails
                .Include(d => d.Room)                   // ✔ JOIN bảng Room
                .AsNoTracking()
                .Select(d => new PromotionDetailListDto
                {
                    PromotionDetailId = d.PromotionDetailID,
                    RoomId = d.RoomId,
                    RoomName = d.Room != null ? d.Room.RoomName : string.Empty,
                    DiscountPercent = d.DiscountPercent
                })
                .ToListAsync();

            return Ok(details);
        }

        // GET: api/PromotionDetail/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PromotionDetailListDto>> GetById(int id)
        {
            var d = await _context.PromotionDetails
                .Include(x => x.Room)
                .FirstOrDefaultAsync(x => x.PromotionDetailID == id);

            if (d == null) return NotFound();

            var dto = new PromotionDetailListDto
            {
                PromotionDetailId = d.PromotionDetailID,
                RoomId = d.RoomId,
                RoomName = d.Room != null ? d.Room.RoomName : string.Empty,
                DiscountPercent = d.DiscountPercent
            };

            return Ok(dto);
        }
    }
}
