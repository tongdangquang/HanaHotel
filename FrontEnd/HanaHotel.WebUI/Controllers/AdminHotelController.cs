using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.DtoLayer.DTOs.HotelDTO;
using HanaHotel.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminHotelController : Controller
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl;
		private readonly IWebHostEnvironment _env;
		private readonly DataContext _db;
		private readonly ILogger<AdminHotelController> _logger;

		public AdminHotelController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings, IWebHostEnvironment env, ILogger<AdminHotelController> logger, DataContext db)
		{
			_httpClientFactory = httpClientFactory;
			_apiUrl = appSettings.Value.urlAPI?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(appSettings));
			_env = env;
			_logger = logger;
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var client = _httpClientFactory.CreateClient();
			var response = await client.GetAsync($"{_apiUrl}/api/Hotels");
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var values = JsonConvert.DeserializeObject<List<HotelDto>>(json);
				return View(values ?? new List<HotelDto>());
			}
			_logger.LogWarning("Index: API returned {Status}", response.StatusCode);
			return View(new List<HotelDto>());
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			// ensure Manager nested object exists for binding
			var model = new HotelDto { Manager = new UserDto() };

			// Load managers (RoleId == 1) và rooms trực tiếp từ DataContext để đổ vào dropdown/checkbox
			var managers = _db.Users
				.Where(x => x.RoleId == 1)
				.Select(u => new UserDto
				{
					Id = u.Id,
					Name = u.Name,
					Email = u.Email,
					PhoneNumber = u.PhoneNumber
				})
				.ToList();

			var rooms = _db.Rooms
				.Select(r => new RoomDto
				{
					Id = r.Id,
					RoomName = r.RoomName,
					Price = r.Price,
					BedCount = r.BedCount,
					// Status field in RoomDto exists but status now lives on HotelDetail; set default 0
					Status = 0
				})
				.ToList();

			ViewBag.Users = managers;
			ViewBag.ManagerId = managers; // giữ compatibility nếu view dùng tên này
			ViewBag.Rooms = rooms;

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(HotelDto model)
		{
			if (!ModelState.IsValid)
			{
				await PopulateUsersAndRoomsForView();
				return View(model);
			}

			try
			{
				// Map HotelDto -> entity và lưu vào DB trực tiếp
				var hotelEntity = new HanaHotel.EntityLayer.Concrete.Hotel
				{
					HotelName = model.HotelName ?? string.Empty,
					Status = (HanaHotel.EntityLayer.Concrete.HotelStatus)model.Status,
					Description = model.Description,
					Size = model.Size ?? 0,
					Address = model.Address ?? string.Empty,
					PhoneNumber = model.PhoneNumber,
					EmployeeCount = model.EmployeeCount ?? 0,
					ManagerId = model.Manager?.Id ?? 0
				};

				_db.Hotels.Add(hotelEntity);
				await _db.SaveChangesAsync();

				var hotelId = hotelEntity.Id;
				_logger.LogInformation("Created hotel locally with id {HotelId}", hotelId);

				// Đọc checkbox SelectedRoomIds từ form và tạo HotelDetail
				var selected = Request.Form["SelectedRoomIds"].ToArray();
				if (selected != null && selected.Length > 0)
				{
					var details = selected
						.Select(s =>
						{
							if (!int.TryParse(s, out var rid)) return (HanaHotel.EntityLayer.Concrete.HotelDetail?)null;
							var countStr = Request.Form[$"RoomCount_{rid}"].FirstOrDefault();
							int cnt = 1;
							if (!string.IsNullOrWhiteSpace(countStr) && int.TryParse(countStr, out var parsed)) cnt = Math.Max(1, parsed);
							return new HanaHotel.EntityLayer.Concrete.HotelDetail
							{
								HotelId = hotelId,
								RoomId = rid,
								RoomCount = cnt,
								Status = HanaHotel.EntityLayer.Concrete.RoomStatus.Available
							};
						})
						.Where(x => x != null)
						.Select(x => x!)
						.ToList();

					if (details.Any())
					{
						_db.HotelDetails.AddRange(details);
						await _db.SaveChangesAsync();
						_logger.LogInformation("Saved {Count} HotelDetail records for hotel {HotelId}", details.Count, hotelId);
					}
				}

				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Create (direct DB) exception");
				ModelState.AddModelError(string.Empty, "Lỗi khi tạo khách sạn (DB).");
				await PopulateUsersAndRoomsForView();
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			// Load hotel + related details from local DB to avoid API inconsistencies
			var hotelEntity = await _db.Hotels
				.Include(h => h.HotelDetails)
				.FirstOrDefaultAsync(h => h.Id == id);

			if (hotelEntity == null)
			{
				_logger.LogWarning("Edit: hotel {Id} not found in DB", id);
				return RedirectToAction(nameof(Index));
			}

			// Map to HotelDto used by the view
			var dto = new HotelDto
			{
				Id = hotelEntity.Id,
				HotelName = hotelEntity.HotelName,
				Status = (int)hotelEntity.Status,
				Description = hotelEntity.Description,
				Size = (float?)hotelEntity.Size,
				Address = hotelEntity.Address,
				PhoneNumber = hotelEntity.PhoneNumber,
				EmployeeCount = hotelEntity.EmployeeCount,
				Manager = hotelEntity.ManagerId > 0 ? new UserDto { Id = hotelEntity.ManagerId } : new UserDto(),
				HotelDetails = hotelEntity.HotelDetails?.Select(hd => new HotelDetailDto
				{
					Id = hd.Id,
					HotelId = hd.HotelId,
					RoomId = hd.RoomId,
					RoomCount = hd.RoomCount
				}).ToList() ?? new List<HotelDetailDto>()
			};

			// Populate dropdowns / checkbox lists from local DB (managers + rooms)
			var managers = _db.Users
				.Where(x => x.RoleId == 1)
				.Select(u => new UserDto { Id = u.Id, Name = u.Name, Email = u.Email, PhoneNumber = u.PhoneNumber })
				.ToList();

			var rooms = _db.Rooms
				.Select(r => new RoomDto { Id = r.Id, RoomName = r.RoomName, Price = r.Price, BedCount = r.BedCount, Status = 0 })
				.ToList();

			ViewBag.Users = managers;
			ViewBag.Rooms = rooms;

			// Selected room ids for checkbox pre-check
			var selectedRoomIds = dto.HotelDetails?.Select(hd => hd.RoomId).ToList() ?? new List<int>();
			ViewBag.SelectedRoomIds = selectedRoomIds;

			var roomCounts = dto.HotelDetails?.ToDictionary(hd => hd.RoomId, hd => hd.RoomCount) ?? new Dictionary<int,int>();
			ViewBag.RoomCounts = roomCounts;

			return View(dto);
		}

		// Replace the Edit POST method with this transactional implementation
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, HotelDto model)
		{
			if (id != model.Id) return BadRequest();

			if (!ModelState.IsValid)
			{
				await PopulateUsersAndRoomsForView();
				return View(model);
			}

			// read selected rooms and counts from form first
			var selected = Request.Form["SelectedRoomIds"].ToArray();
			var roomSelections = new List<(int RoomId, int Count)>();
			if (selected != null && selected.Length > 0)
			{
				foreach (var s in selected)
				{
					if (!int.TryParse(s, out var rid)) continue;
					var countStr = Request.Form[$"RoomCount_{rid}"].FirstOrDefault();
					int cnt = 1;
					if (!string.IsNullOrWhiteSpace(countStr) && int.TryParse(countStr, out var parsed)) cnt = Math.Max(1, parsed);
					roomSelections.Add((rid, cnt));
				}
			}

			using var tx = await _db.Database.BeginTransactionAsync();
			try
			{
				var hotelEntity = await _db.Hotels.FirstOrDefaultAsync(h => h.Id == id);
				if (hotelEntity == null) return NotFound();

				hotelEntity.HotelName = (model.HotelName ?? hotelEntity.HotelName).Trim();
				hotelEntity.Status = (HanaHotel.EntityLayer.Concrete.HotelStatus)model.Status;
				hotelEntity.Description = model.Description;
				hotelEntity.Size = model.Size ?? hotelEntity.Size;
				hotelEntity.Address = model.Address ?? hotelEntity.Address;
				hotelEntity.PhoneNumber = model.PhoneNumber;
				hotelEntity.EmployeeCount = model.EmployeeCount ?? hotelEntity.EmployeeCount;
				hotelEntity.ManagerId = model.Manager?.Id ?? hotelEntity.ManagerId;

				_db.Hotels.Update(hotelEntity);
				await _db.SaveChangesAsync();

				// Replace HotelDetails atomically: delete existing then add new (all within tx)
				var existingDetails = await _db.HotelDetails.Where(hd => hd.HotelId == id).ToListAsync();
				if (existingDetails.Any())
				{
					_db.HotelDetails.RemoveRange(existingDetails);
					await _db.SaveChangesAsync();
				}

				if (roomSelections.Any())
				{
					var newDetails = roomSelections
						.Select(rs => new HanaHotel.EntityLayer.Concrete.HotelDetail
						{
							HotelId = id,
							RoomId = rs.RoomId,
							RoomCount = rs.Count,
							Status = HanaHotel.EntityLayer.Concrete.RoomStatus.Available
						})
						.ToList();

					_db.HotelDetails.AddRange(newDetails);
					await _db.SaveChangesAsync();
					_logger.LogInformation("Updated HotelDetails for hotel {HotelId}, count {Count}", id, newDetails.Count);
				}
				else
				{
					_logger.LogInformation("No rooms selected for hotel {HotelId}; existing HotelDetails removed", id);
				}

				await tx.CommitAsync();
				return RedirectToAction(nameof(Index));
			}
			catch (Exception ex)
			{
				await tx.RollbackAsync();
				_logger.LogError(ex, "Edit (direct DB) exception for hotel {HotelId} - transaction rolled back", id);
				ModelState.AddModelError(string.Empty, "Lỗi khi cập nhật khách sạn (DB). Thử lại hoặc kiểm tra logs.");
				await PopulateUsersAndRoomsForView();
				return View(model);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var client = _httpClientFactory.CreateClient();
			var response = await client.GetAsync($"{_apiUrl}/api/Hotels/{id}");
			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				var value = JsonConvert.DeserializeObject<HotelDto>(json);
				if (value != null) return View(value);
			}
			_logger.LogWarning("Details: hotel {Id} not found or API error", id);
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Delete(int id)
		{
			var client = _httpClientFactory.CreateClient();
			var response = await client.DeleteAsync($"{_apiUrl}/api/Hotels/{id}");
			if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NoContent)
			{
				_logger.LogInformation("Deleted hotel {Id} via API", id);
				return RedirectToAction(nameof(Index));
			}
			_logger.LogError("Delete API failed for {Id}: {Status}", id, response.StatusCode);
			TempData["Error"] = "Xóa thất bại.";
			return RedirectToAction(nameof(Index));
		}

		// helper to populate users & rooms for Create/Edit views
		private async Task PopulateUsersAndRoomsForView()
		{
			var client = _httpClientFactory.CreateClient();

			try
			{
				var usersResp = await client.GetAsync($"{_apiUrl}/api/Users");
				if (!usersResp.IsSuccessStatusCode)
					usersResp = await client.GetAsync($"{_apiUrl}/api/User");

				if (usersResp.IsSuccessStatusCode)
				{
					var usersJson = await usersResp.Content.ReadAsStringAsync();
					var users = JsonConvert.DeserializeObject<List<UserDto>>(usersJson) ?? new List<UserDto>();
					ViewBag.Users = users;
				}
				else
				{
					ViewBag.Users = new List<UserDto>();
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to load users for view");
				ViewBag.Users = new List<UserDto>();
			}

			try
			{
				var roomsResp = await client.GetAsync($"{_apiUrl}/api/Room");
				if (roomsResp.IsSuccessStatusCode)
				{
					var roomsJson = await roomsResp.Content.ReadAsStringAsync();
					var rooms = JsonConvert.DeserializeObject<List<RoomDto>>(roomsJson) ?? new List<RoomDto>();
					ViewBag.Rooms = rooms;
				}
				else
				{
					ViewBag.Rooms = new List<RoomDto>();
				}
			}
			catch (Exception ex)
			{
				_logger.LogWarning(ex, "Failed to load rooms for view");
				ViewBag.Rooms = new List<RoomDto>();
			}
		}
	}
}