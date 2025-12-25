using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GuestController : ControllerBase
    {
        private readonly IGuestService _guestService;
        public GuestController(IGuestService guestService)
        {
            _guestService = guestService;
        }

        [HttpGet]
        public IActionResult GetGuests()
        {
            var guest = _guestService.TGetList();
            return Ok(guest);
        }

        [HttpGet("{id}")]
        public IActionResult GetGuest(int id)
        {
            var guest = _guestService.TGetByID(id);
            return Ok(guest);
        }

        [HttpPut]
        public IActionResult UpdateGuest(Guest guest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _guestService.TUpdate(guest);
            return Ok("Updated Successfully");
        }

        [HttpPost]
        public IActionResult AddGuest(Guest guest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _guestService.TInsert(guest);
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteGuest(int id)
        {
            var guest = _guestService.TGetByID(id);
            if (guest == null)
                return NotFound();

            _guestService.TDelete(guest);
            return NoContent();
        }
    }
}
