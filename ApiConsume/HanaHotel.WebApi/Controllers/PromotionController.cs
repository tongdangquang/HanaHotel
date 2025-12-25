using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DtoLayer.DTOs.PromotionDTO;
using Microsoft.AspNetCore.Mvc;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PromotionController : ControllerBase
    {
        private readonly IPromotionService _promotionService;

        public PromotionController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _promotionService.GetAllAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var dto = await _promotionService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PromotionCreateDto dto)
        {
            await _promotionService.AddAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = 0 }, null);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] PromotionCreateDto dto)
        {
            await _promotionService.UpdateAsync(id, dto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _promotionService.DeleteAsync(id);
            return NoContent();
        }
    }
}
