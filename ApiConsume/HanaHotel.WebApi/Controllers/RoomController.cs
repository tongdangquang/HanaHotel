using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.DataAccessLayer.Abstract;
using HanaHotel.DtoLayer.DTOs.RoomDTO;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.Extensions.Logging;

namespace HanaHotel.WebApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class RoomController : ControllerBase
	{
		private readonly IRoomService _roomService;
		private readonly IImageDal _imageDal;
		private readonly IMapper _mapper;
		private readonly ILogger<RoomController> _logger;

		public RoomController(IRoomService roomService, IImageDal imageDal, IMapper mapper, ILogger<RoomController> logger)
		{
			_roomService = roomService;
			_imageDal = imageDal;
			_mapper = mapper;
			_logger = logger;
		}

		// GET: api/room
		[HttpGet]
		public IActionResult GetRooms()
		{
			var rooms = _roomService.TGetList();
			var images = _imageDal.GetList();

			var result = rooms.Select(r =>
			{
				var dto = _mapper.Map<ResultRoomDTO>(r);
				dto.ImagePaths = images
					.Where(i => i.RoomId == r.Id)
					.Select(i => i.ImagePath)
					.ToList();
				return dto;
			}).ToList();

			return Ok(result);
		}

		// GET: api/room/5
		[HttpGet("{id}")]
		public IActionResult GetRoom(int id)
		{
			var room = _roomService.TGetByID(id);
			if (room == null)
				return NotFound();

			var dto = _mapper.Map<ResultRoomDTO>(room);
			dto.ImagePaths = _imageDal.GetList()
				.Where(i => i.RoomId == room.Id)
				.Select(i => i.ImagePath)
				.ToList();

			return Ok(dto);
		}

		// PUT: api/room
		[HttpPut]
		public IActionResult UpdateRoom([FromBody] UpdateRoomDTO updateRoomDTO)
		{
			if (updateRoomDTO == null)
			{
				_logger.LogWarning("UpdateRoom: payload is null");
				return BadRequest("Payload is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning("UpdateRoom: invalid model state");
				return BadRequest(ModelState);
			}

			_logger.LogInformation("UpdateRoom called for RoomId={RoomId}. ImagePathsCount={Count}, RemoveImageIdsCount={RidCount}, RemoveImagePathsCount={RpathCount}",
				updateRoomDTO.Id,
				updateRoomDTO.ImagePaths?.Count ?? 0,
				updateRoomDTO.RemoveImageIds?.Length ?? 0,
				updateRoomDTO.RemoveImagePaths?.Length ?? 0);

			if (updateRoomDTO.RemoveImageIds != null && updateRoomDTO.RemoveImageIds.Any())
			{
				foreach (var imgId in updateRoomDTO.RemoveImageIds)
				{
					try
					{
						var img = _imageDal.GetByID(imgId);
						if (img != null)
						{
							_imageDal.Delete(img);
							_logger.LogInformation("Deleted Image record Id={ImageId}", imgId);
						}
						else
						{
							_logger.LogWarning("Image id {ImageId} not found for deletion", imgId);
						}
					}
					catch (System.Exception ex)
					{
						_logger.LogError(ex, "Error deleting image id {ImageId}", imgId);
					}
				}
			}

			if (updateRoomDTO.RemoveImagePaths != null && updateRoomDTO.RemoveImagePaths.Any())
			{
				var all = _imageDal.GetList();
				foreach (var path in updateRoomDTO.RemoveImagePaths)
				{
					try
					{
						var img = all.FirstOrDefault(i => string.Equals(i.ImagePath, path, System.StringComparison.OrdinalIgnoreCase) && i.RoomId == updateRoomDTO.Id);
						if (img != null)
						{
							_imageDal.Delete(img);
							_logger.LogInformation("Deleted Image record by path {Path}", path);
						}
						else
						{
							_logger.LogWarning("Image path {Path} not found for RoomId={RoomId}", path, updateRoomDTO.Id);
						}
					}
					catch (System.Exception ex)
					{
						_logger.LogError(ex, "Error deleting image by path {Path}", path);
					}
				}
			}

			if (updateRoomDTO.ImagePaths != null && updateRoomDTO.ImagePaths.Any())
			{
				foreach (var p in updateRoomDTO.ImagePaths)
				{
					try
					{
						var image = new Image { ImagePath = p, RoomId = updateRoomDTO.Id };
						_imageDal.Insert(image);
						_logger.LogInformation("Inserted Image record path={Path} RoomId={RoomId}", p, updateRoomDTO.Id);
					}
					catch (System.Exception ex)
					{
						_logger.LogError(ex, "Error inserting image path {Path}", p);
					}
				}
			}

			var value = _mapper.Map<Room>(updateRoomDTO);
			try
			{
				_roomService.TUpdate(value);
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "Error updating room Id={RoomId}", updateRoomDTO.Id);
				return StatusCode(500, "Error updating room");
			}

			return Ok("Updated Successfully");
		}

		// POST: api/room
		[HttpPost]
		public IActionResult AddRoom([FromBody] RoomAddDTO roomAddDTO)
		{
			if (roomAddDTO == null)
			{
				_logger.LogWarning("AddRoom: payload is null");
				return BadRequest("Payload is null");
			}

			if (!ModelState.IsValid)
			{
				_logger.LogWarning("AddRoom: invalid model state");
				return BadRequest(ModelState);
			}

			_logger.LogInformation("AddRoom called. ImagePathsCount={Count}", roomAddDTO.ImagePaths?.Count ?? 0);

			Room values;
			try
			{
				values = _mapper.Map<Room>(roomAddDTO);
				_roomService.TInsert(values);
			}
			catch (System.Exception ex)
			{
				_logger.LogError(ex, "Error inserting room");
				return StatusCode(500, "Error inserting room");
			}

			if (roomAddDTO.ImagePaths != null && roomAddDTO.ImagePaths.Any())
			{
				foreach (var p in roomAddDTO.ImagePaths)
				{
					try
					{
						var image = new Image { ImagePath = p, RoomId = values.Id };
						_imageDal.Insert(image);
						_logger.LogInformation("Inserted Image record path={Path} RoomId={RoomId}", p, values.Id);
					}
					catch (System.Exception ex)
					{
						_logger.LogError(ex, "Error inserting image path {Path}", p);
					}
				}
			}

			return Ok();
		}

		// DELETE: api/room/5
		[HttpDelete("{id}")]
		public IActionResult DeleteRoom(int id)
		{
			var room = _roomService.TGetByID(id);
			if (room == null)
			{
				return NotFound();
			}

			var images = _imageDal.GetList().Where(i => i.RoomId == id).ToList();
			foreach (var img in images)
			{
				try { _imageDal.Delete(img); } catch { /* log if needed */ }
			}

			_roomService.TDelete(room);
			return NoContent();
		}
	}
}