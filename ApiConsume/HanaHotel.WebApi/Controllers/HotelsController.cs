using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.DtoLayer.DTOs.HotelDTO;

namespace HanaHotel.WebApi.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class HotelsController : ControllerBase
	{
		private readonly IHotelService _hotelService;
		private readonly IHotelDetailService _hotelDetailService;
		private readonly ILogger<HotelsController> _logger;

		public HotelsController(
			IHotelService hotelService,
			IHotelDetailService hotelDetailService,
			ILogger<HotelsController> logger)
		{
			_hotelService = hotelService;
			_hotelDetailService = hotelDetailService;
			_logger = logger;
		}

		// GET: api/Hotels
		// Return only properties from Hotel (+ Manager user minimal + details minimal)
		[HttpGet]
		public ActionResult<IEnumerable<HotelDto>> GetList()
		{
			var hotels = _hotelService.TGetList();
			var dtos = hotels.Select(MapHotel).ToList();
			return Ok(dtos);
		}

		// GET: api/Hotels/5
		[HttpGet("{id:int}")]
		public ActionResult<HotelDto> GetById(int id)
		{
			var hotel = _hotelService.TGetByID(id);
			if (hotel == null) return NotFound();
			return Ok(MapHotel(hotel));
		}

		// POST: api/Hotels
		// Accept minimal HotelDto input (uses same fields); maps to entity for insert
		[HttpPost]
		public ActionResult<HotelDto> Create([FromBody] HotelDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);

			var entity = new Hotel
			{
				HotelName = dto.HotelName ?? string.Empty,
				Status = (HotelStatus)dto.Status,
				Description = dto.Description,
				Size = dto.Size ?? 0,
				Address = dto.Address ?? string.Empty,
				PhoneNumber = dto.PhoneNumber,
				EmployeeCount = dto.EmployeeCount ?? 0,
				ManagerId = dto.Manager?.Id ?? 0
			};

			_hotelService.TInsert(entity);

			// Map back (Id should be set by DAL)
			dto.Id = entity.Id;
			return CreatedAtAction(nameof(GetById), new { id = dto.Id }, dto);
		}

		// PUT: api/Hotels/5
		[HttpPut("{id:int}")]
		public IActionResult Update(int id, [FromBody] HotelDto dto)
		{
			if (!ModelState.IsValid) return BadRequest(ModelState);
			if (id != dto.Id) return BadRequest("Id mismatch");

			var existing = _hotelService.TGetByID(id);
			if (existing == null) return NotFound();

			existing.HotelName = dto.HotelName ?? existing.HotelName;
			existing.Status = (HotelStatus)dto.Status;
			existing.Description = dto.Description;
			existing.Size = dto.Size ?? existing.Size;
			existing.Address = dto.Address ?? existing.Address;
			existing.PhoneNumber = dto.PhoneNumber;
			existing.EmployeeCount = dto.EmployeeCount ?? existing.EmployeeCount;
			existing.ManagerId = dto.Manager?.Id ?? existing.ManagerId;

			_hotelService.TUpdate(existing);
			return NoContent();
		}

		// DELETE: api/Hotels/5
		[HttpDelete("{id:int}")]
		public IActionResult Delete(int id)
		{
			var hotel = _hotelService.TGetByID(id);
			if (hotel == null) return NotFound();

			_hotelService.TDelete(hotel);
			return NoContent();
		}

		// GET: api/Hotels/5/details
		// Return only HotelDetail fields + nested Room minimal fields
		[HttpGet("{hotelId:int}/details")]
		public ActionResult<IEnumerable<HotelDetailDto>> GetDetailsByHotel(int hotelId)
		{
			var details = _hotelDetailService.TGetList()
				.Where(d => d.HotelId == hotelId)
				.ToList();

			var dtos = details.Select(MapHotelDetail).ToList();
			return Ok(dtos);
		}

		// PUT: api/Hotels/5/details
		// Replace details for a hotel: deletes existing details and inserts provided ones
		[HttpPut("{hotelId:int}/details")]
		public IActionResult UpdateDetails(int hotelId, [FromBody] IEnumerable<HotelDetailDto> details)
		{
			var hotel = _hotelService.TGetByID(hotelId);
			if (hotel == null) return NotFound();

			// delete existing details
			var existing = _hotelDetailService.TGetList().Where(d => d.HotelId == hotelId).ToList();
			foreach (var e in existing)
				_hotelDetailService.TDelete(e);

			// insert new
			foreach (var dto in details)
			{
				var entity = new HotelDetail
				{
					HotelId = hotelId,
					RoomId = dto.RoomId,
					RoomCount = dto.RoomCount
				};
				_hotelDetailService.TInsert(entity);
			}

			return NoContent();
		}

		// Mapping helpers (only map fields required by user)
		private static HotelDto MapHotel(Hotel h)
		{
			return new HotelDto
			{
				Id = h.Id,
				HotelName = h.HotelName,
				Status = (int)h.Status,
				Description = h.Description,
				Size = (float?)h.Size,
				Address = h.Address,
				PhoneNumber = h.PhoneNumber,
				EmployeeCount = h.EmployeeCount,
				Manager = h.Manager == null ? null : new UserDto
				{
					Id = h.Manager.Id,
					Name = h.Manager.Name,
					Email = h.Manager.Email,
					PhoneNumber = h.Manager.PhoneNumber
				},
				HotelDetails = h.HotelDetails?.Select(MapHotelDetail).ToList() ?? new List<HotelDetailDto>()
			};
		}

		private static HotelDetailDto MapHotelDetail(HotelDetail d)
		{
			return new HotelDetailDto
			{
				Id = d.Id,
				HotelId = d.HotelId,
				RoomId = d.RoomId,
				RoomCount = d.RoomCount,
				Room = d.Room == null ? null : new RoomDto
				{
					Id = d.Room.Id,
					RoomName = d.Room.RoomName,
					Price = d.Room.Price,
					BedCount = d.Room.BedCount,
					// Status previously read from Room; now the status lives on HotelDetail
					Status = (int)d.Status
				}
			};
		}
	}
}