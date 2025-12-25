using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.BookingDTO;
using HanaHotel.WebUI.DTOs.RoomDTO;
using HanaHotel.WebUI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace HanaHotel.WebUI.Controllers
{
	public class BookingController : Controller
	{
		private readonly DataContext _db;

		public BookingController(DataContext db)
		{
			_db = db;
		}

		[HttpGet]
		public async Task<IActionResult> Index()
		{
			var availableHotelDetails = await _db.HotelDetails
				.Include(hd => hd.Room)
				.Include(hd => hd.Hotel)
				.Where(hd => hd.Room != null && hd.Status == RoomStatus.Available)
				.ToListAsync();

			var availableRooms = availableHotelDetails
				.GroupBy(hd => hd.Room!.Id)
				.Select(g =>
				{
					var hd = g.First();
					var r = hd.Room!;
					return new ResultRoomDTO
					{
						Id = r.Id,
						RoomName = r.RoomName,
						Status = hd.Status,
						Description = r.Description,
						Size = r.Size,
						Price = r.Price,
						BedCount = r.BedCount,
						HotelName = hd.Hotel?.HotelName,
						RoomCount = g.Sum(x => x.RoomCount),
						HotelDetailId = hd.Id // pick representative HotelDetailId (first in group)
					};
				})
				.ToList();

			var imagesList = await _db.Images.ToListAsync();
			foreach (var room in availableRooms)
			{
				room.ImagePaths = imagesList
					.Where(img => img.RoomId == room.Id)
					.Select(img => img.ImagePath)
					.ToList();
			}

			// Collect room ids to lookup services and promotions in bulk
			var roomIds = availableRooms.Select(r => r.Id).ToList();

			// Map roomId -> list of service names via ServiceDetails -> Service
			var serviceDetails = await (from sd in _db.ServiceDetails.Where(sd => roomIds.Contains(sd.RoomId))
										join s in _db.Services on sd.ServiceId equals s.Id
										select new
										{
											sd.RoomId,
											ServiceName = s.ServiceName
										}).ToListAsync();

			var roomIdToServices = serviceDetails
				.GroupBy(sd => sd.RoomId)
				.ToDictionary(
					g => g.Key,
					g => g.Select(x => x.ServiceName)
						  .Where(s => !string.IsNullOrWhiteSpace(s))
						  .Distinct()
						  .ToList()
				);

			// Map roomId -> promotion (choose active promotions only; pick best one if multiple)
			var now = DateTime.UtcNow.Date;
			var promotionDetails = await _db.PromotionDetails
				.Include(pd => pd.Promotion)
				.Where(pd => roomIds.Contains(pd.RoomId) && pd.Promotion != null && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
				.ToListAsync();

			var roomIdToPromotion = promotionDetails
				.GroupBy(pd => pd.RoomId)
				.ToDictionary(g =>
				{
					return g.Key;
				},
				g =>
				{
					var best = g.OrderByDescending(x => x.DiscountPercent)
								.ThenByDescending(x => x.Promotion.DiscountAmount)
								.FirstOrDefault();
					return best;
				});

			// Apply mappings into DTOs
			foreach (var room in availableRooms)
			{
				if (roomIdToServices.TryGetValue(room.Id, out var services) && services.Any())
				{
					room.Services = services;
				}

				if (roomIdToPromotion.TryGetValue(room.Id, out var promoDetail) && promoDetail != null)
				{
					room.PromotionName = promoDetail.Promotion?.PromotionName;
					room.PromotionDiscountAmount = promoDetail.Promotion?.DiscountAmount;
					room.PromotionDiscountPercent = promoDetail.DiscountPercent;
				}
			}

			var images = await _db.Images.ToListAsync();

			// Ensure ImagePaths are filled (fallback to images loaded earlier if necessary)
			foreach (var room in availableRooms)
			{
				if (room.ImagePaths == null || !room.ImagePaths.Any())
				{
					room.ImagePaths = images.Where(img => img.RoomId == room.Id).Select(img => img.ImagePath).ToList();
				}
			}

			// New: For search dropdowns used in Index view
			var hotelNames = await _db.Hotels
				.AsNoTracking()
				.Where(h => !string.IsNullOrEmpty(h.HotelName))
				.Select(h => h.HotelName)
				.Distinct()
				.OrderBy(n => n)
				.ToListAsync();

			var roomNames = await _db.Rooms
				.AsNoTracking()
				.Where(r => !string.IsNullOrEmpty(r.RoomName))
				.Select(r => r.RoomName)
				.Distinct()
				.OrderBy(n => n)
				.ToListAsync();

			ViewBag.Hotels = hotelNames;
			ViewBag.RoomNames = roomNames;

			// If a default hotel exists (we use the first in the list), filter initial availableRooms.
			var defaultHotel = hotelNames.FirstOrDefault();
			if (!string.IsNullOrWhiteSpace(defaultHotel))
			{
				var defaultHotelLower = defaultHotel.Trim().ToLower();

				var hdList = await _db.HotelDetails
					.Include(hd => hd.Hotel)
					.Include(hd => hd.Room)
					.Where(hd => hd.Hotel != null
								 && hd.Hotel.HotelName != null
								 && hd.Hotel.HotelName.ToLower().Contains(defaultHotelLower)
								 && hd.Room != null
								 && hd.Status == RoomStatus.Available) // Status on HotelDetail
					.ToListAsync();

				if (hdList != null && hdList.Any())
				{
					var roomIdsFromHd = hdList.Select(hd => hd.Room!.Id).Distinct().ToList();

					// get services & promotions for these rooms too
					var svcDetailsForHd = await (from sd in _db.ServiceDetails.Where(sd => roomIdsFromHd.Contains(sd.RoomId))
												join s in _db.Services on sd.ServiceId equals s.Id
												select new { sd.RoomId, ServiceName = s.ServiceName }).ToListAsync();

					var svcMap = svcDetailsForHd
						.GroupBy(x => x.RoomId)
						.ToDictionary(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

					var promoDetailsForHd = await _db.PromotionDetails
						.Include(pd => pd.Promotion)
						.Where(pd => roomIdsFromHd.Contains(pd.RoomId) && pd.Promotion != null && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
						.ToListAsync();

					var promoMap = promoDetailsForHd
						.GroupBy(pd => pd.RoomId)
						.ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.DiscountPercent).ThenByDescending(x => x.Promotion.DiscountAmount).FirstOrDefault());

					// Build results similar to SearchRooms projection so the partial gets same DTO shape
					var results = hdList
						.Where(hd => hd.Room != null)
						.Select(hd => new ResultRoomDTO
						{
							Id = hd.Room!.Id,
							RoomName = hd.Room!.RoomName,
							Status = hd.Status,
							Description = hd.Room!.Description,
							Size = hd.Room!.Size,
							Price = hd.Room!.Price,
							BedCount = hd.Room!.BedCount,
							ImagePaths = images.Where(i => i.RoomId == hd.Room.Id).Select(i => i.ImagePath).ToList(),
							HotelName = hd.Hotel?.HotelName ?? defaultHotel,
							Services = svcMap.ContainsKey(hd.Room.Id) ? svcMap[hd.Room.Id] : new List<string>(),
							PromotionName = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.Promotion?.PromotionName : null,
							PromotionDiscountAmount = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.Promotion?.DiscountAmount : null,
							PromotionDiscountPercent = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.DiscountPercent : null,
							RoomCount = hd.RoomCount,
							HotelDetailId = hd.Id
						})
						.GroupBy(r => r.Id)
						.Select(g => g.First())
						.ToList();

					availableRooms = results;
				}
				else
				{
					// Fallback: keep original availableRooms but only mark HotelName if mapping exists
					availableRooms = availableRooms
						.Where(r => string.Equals(r.HotelName ?? string.Empty, defaultHotel, System.StringComparison.OrdinalIgnoreCase))
						.ToList();
				}
			}

			var model = new CreateBookingDTO
			{
				AvailableRooms = availableRooms
			};

			return View(model);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AddBooking(CreateBookingDTO dto)
		{
			// Basic server-side validation: must choose at least one room
			if (dto.RoomDetails == null || !dto.RoomDetails.Any(x => x.Quantity > 0))
			{
				ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất 1 phòng.");
			}

			if (!ModelState.IsValid)
			{
				// repopulate available rooms for redisplay using HotelDetails (status moved)
				var availableHotelDetails = await _db.HotelDetails
					.Include(hd => hd.Room)
					.Include(hd => hd.Hotel)
					.Where(hd => hd.Room != null && hd.Status == RoomStatus.Available)
					.ToListAsync();

				dto.AvailableRooms = availableHotelDetails
					.GroupBy(hd => hd.Room!.Id)
					.Select(g =>
					{
						var hd = g.First();
						var r = hd.Room!;
						return new ResultRoomDTO
						{
							Id = r.Id,
							RoomName = r.RoomName,
							Status = hd.Status,
							Description = r.Description,
							Size = r.Size,
							Price = r.Price,
							BedCount = r.BedCount,
							HotelName = hd.Hotel?.HotelName,
							RoomCount = g.Sum(x => x.RoomCount),
							HotelDetailId = hd.Id
						};
					})
					.ToList();

				return View("Index", dto);
			}

			// Get current user id from claims (IdentityUser<int>)
			int currentUserId = 0;
			var idClaim = User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
			if (!string.IsNullOrEmpty(idClaim) && int.TryParse(idClaim, out var parsedId))
			{
				currentUserId = parsedId;
			}

			// Choose a representative RoomId for Booking (optional).
			var firstSelected = dto.RoomDetails.FirstOrDefault(x => x.Quantity > 0);
			var representativeRoomId = firstSelected?.RoomId ?? dto.RoomId ?? 0;

			var booking = new Booking
			{
				FullName = dto.FullName,
				Email = dto.Email,
				Phone = dto.Phone,
				CheckInDate = dto.CheckInDate,
				CheckOutDate = dto.CheckOutDate,
				BookingDate = dto.BookingDate ?? DateTime.UtcNow,
				Status = dto.Status ?? BookingStatus.Pending,
				AdditionalRequest = dto.AdditionalRequest,
				UserId = currentUserId,
				RoomId = representativeRoomId
			};

			_db.Bookings.Add(booking);
			await _db.SaveChangesAsync();

			// Save room details referencing booking.Id
			foreach (var rd in dto.RoomDetails.Where(x => x.Quantity > 0))
			{
				var roomDetail = new RoomDetail
				{
					RoomId = rd.RoomId,
					Quantity = rd.Quantity,
					AdultAmount = rd.AdultAmount,
					ChildrenAmount = rd.ChildrenAmount,
					BookingId = booking.Id,
					HotelDetailId = rd.HotelDetailId
				};
				_db.RoomDetails.Add(roomDetail);
			}

			await _db.SaveChangesAsync();

			// Redirect to MyBookings or Index as you prefer
			return RedirectToAction("MyBookings", "Booking");
		}

		[HttpGet]
		public async Task<IActionResult> MyBookings()
		{
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
			{
				return RedirectToAction("Index", "Login");
			}

			// load bookings for user
			var bookings = await _db.Bookings
				.Where(b => b.UserId == userId)
				.OrderByDescending(b => b.BookingDate)
				.ToListAsync();

			if (!bookings.Any())
				return View("MyBookings", new List<BookingRoomViewModel>());

			var bookingIds = bookings.Select(b => b.Id).ToList();
			var roomDetails = await _db.RoomDetails
				.Where(rd => bookingIds.Contains(rd.BookingId))
				.ToListAsync();

			// collect HotelDetailIds (these are IDs of HotelDetail rows, not Hotel.Id)
			var hotelDetailIds = roomDetails
				.Where(rd => rd.HotelDetailId.HasValue)
				.Select(rd => rd.HotelDetailId!.Value)
				.Distinct()
				.ToList();

			var roomIds = roomDetails.Select(rd => rd.RoomId).Distinct().ToList();
			var rooms = await _db.Rooms.Where(r => roomIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r);

			// load HotelDetail -> Hotel mapping to be able to show HotelName and Address
			var hotelDetailMap = new Dictionary<int, HotelDetail>();
			if (hotelDetailIds.Any())
			{
				hotelDetailMap = await _db.HotelDetails
					.Include(hd => hd.Hotel)
					.Where(hd => hotelDetailIds.Contains(hd.Id))
					.ToDictionaryAsync(hd => hd.Id, hd => hd);
			}

			// Load services for involved rooms
			var svcDetails = await (from sd in _db.ServiceDetails.Where(sd => roomIds.Contains(sd.RoomId))
									join s in _db.Services on sd.ServiceId equals s.Id
									select new { sd.RoomId, ServiceName = s.ServiceName }).ToListAsync();

			var svcMap = svcDetails
				.GroupBy(x => x.RoomId)
				.ToDictionary(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

			// Load active promotions for involved rooms (choose best per room)
			var now = DateTime.UtcNow.Date;
			var promoDetails = await _db.PromotionDetails
				.Include(pd => pd.Promotion)
				.Where(pd => roomIds.Contains(pd.RoomId) && pd.Promotion != null && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
				.ToListAsync();

			var promoMap = promoDetails
				.GroupBy(pd => pd.RoomId)
				.ToDictionary(
					g => g.Key,
					g => g.OrderByDescending(x => x.DiscountPercent)
						  .ThenByDescending(x => x.Promotion.DiscountAmount)
						  .FirstOrDefault()
				);

			var rows = new List<BookingRoomViewModel>();

			foreach (var b in bookings)
			{
				var details = roomDetails.Where(rd => rd.BookingId == b.Id).ToList();
				var nights = (int)Math.Max(1, (b.CheckOutDate.Date - b.CheckInDate.Date).TotalDays);

				// prepare DTO list for whole booking
				var rdDtos = new List<RoomDetailDto>();
				foreach (var rd in details)
				{
					rooms.TryGetValue(rd.RoomId, out var room);

					var dto = new RoomDetailDto
					{
						RoomId = rd.RoomId,
						RoomName = room?.RoomName ?? "Phòng",
						Quantity = rd.Quantity,
						AdultAmount = rd.AdultAmount,
						ChildrenAmount = rd.ChildrenAmount,
						Price = room != null ? (decimal)room.Price : 0m
					};

					// attach services if any
					if (svcMap.TryGetValue(rd.RoomId, out var svcs) && svcs != null)
						dto.Services = svcs;

					// attach promotion if active
					if (promoMap.TryGetValue(rd.RoomId, out var pd) && pd != null)
					{
						dto.PromotionName = pd.Promotion?.PromotionName;
						dto.PromotionDiscountPercent = (double)pd.DiscountPercent;
						dto.PromotionDiscountAmount = pd.Promotion?.DiscountAmount;
					}
					rdDtos.Add(dto);
				}

				// create one row per room detail so the list shows each room separately
				foreach (var rd in rdDtos)
				{
					// find corresponding RoomDetail entity for this rd to get HotelDetailId (if any)
					var correspondingRoomDetail = details.FirstOrDefault(d => d.RoomId == rd.RoomId);
					string hotelName = string.Empty;
					string hotelAddress = string.Empty;

					if (correspondingRoomDetail != null && correspondingRoomDetail.HotelDetailId.HasValue)
					{
						if (hotelDetailMap.TryGetValue(correspondingRoomDetail.HotelDetailId.Value, out var hd) && hd?.Hotel != null)
						{
							hotelName = hd.Hotel.HotelName ?? string.Empty;
							hotelAddress = hd.Hotel.Address ?? string.Empty;
						}
					}

					var rowSubtotal = (double)rd.Price * rd.Quantity * nights;
					rows.Add(new BookingRoomViewModel
					{
						BookingId = b.Id,
						BookingDate = b.BookingDate,
						RoomId = rd.RoomId,
						RoomName = rd.RoomName,
						HotelName = hotelName,
						HotelAddress = hotelAddress,
						CheckIn = b.CheckInDate,
						CheckOut = b.CheckOutDate,
						Quantity = rd.Quantity,
						AdultAmount = rd.AdultAmount,
						ChildrenAmount = rd.ChildrenAmount,
						Subtotal = rowSubtotal,
						Status = b.Status.ToString(),
						Email = b.Email,
						Phone = b.Phone,
						AdditionalRequest = b.AdditionalRequest ?? string.Empty,
						BookingRoomDetails = rdDtos // include full booking details for the details modal (now contains services/promo)
					});
				}
			}

			return View("MyBookings", rows);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Cancel(int bookingId)
		{
			// xác thực user
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
				return Unauthorized();

			// tìm booking thuộc user hiện tại
			var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
			if (booking == null)
				return NotFound();

			// nếu đã hủy rồi thì trả về success để client cập nhật UI
			if (booking.Status == BookingStatus.Cancelled)
				return Json(new { success = true, already = true });

			booking.Status = BookingStatus.Cancelled;
			await _db.SaveChangesAsync();

			return Json(new { success = true });
		}

		// NEW: AJAX room search for booking page — returns partial with available room cards
		[HttpGet]
		public async Task<IActionResult> SearchRooms(string? hotelName, string? roomName, DateTime? checkIn, DateTime? checkOut,
													 string? bedCount, double? minSize, double? maxSize, decimal? maxPrice)
		{
			var hdQuery = _db.HotelDetails
				.Include(hd => hd.Hotel)
				.Include(hd => hd.Room)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(hotelName))
			{
				var nameLower = hotelName.Trim().ToLowerInvariant();
				hdQuery = hdQuery.Where(hd => hd.Hotel != null && hd.Hotel.HotelName.ToLower().Contains(nameLower));
			}

			if (!string.IsNullOrWhiteSpace(roomName))
			{
				var rn = roomName.Trim().ToLowerInvariant();
				hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.RoomName.ToLower().Contains(rn));
			}

			if (!string.IsNullOrWhiteSpace(bedCount) && int.TryParse(bedCount.Trim(), out var bedCountInt))
			{
				hdQuery = hdQuery.Where(hd =>
					hd.Room != null && hd.Room.BedCount <= bedCountInt);
			}

			if (minSize.HasValue)
				hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Size >= minSize.Value);
			if (maxSize.HasValue)
				hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Size <= maxSize.Value);

			if (maxPrice.HasValue)
				hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Price <= maxPrice.Value);

			var list = await hdQuery.ToListAsync();

			// availability check using RoomDetails bookings overlapping requested dates
			if (checkIn.HasValue && checkOut.HasValue && checkOut.Value > checkIn.Value)
			{
				var ci = checkIn.Value.Date;
				var co = checkOut.Value.Date;
				var hdIds = list.Select(x => x.Id).ToList();

				var bookedSums = await _db.RoomDetails
					.Where(rd => rd.HotelDetailId.HasValue && hdIds.Contains(rd.HotelDetailId.Value))
					.Join(_db.Bookings,
						  rd => rd.BookingId,
						  b => b.Id,
						  (rd, b) => new { rd.HotelDetailId, rd.Quantity, b.CheckInDate, b.CheckOutDate })
					.Where(x => x.CheckInDate < co && x.CheckOutDate > ci)
					.GroupBy(x => x.HotelDetailId)
					.Select(g => new { HotelDetailId = g.Key, Booked = g.Sum(x => x.Quantity) })
					.ToDictionaryAsync(x => x.HotelDetailId, x => x.Booked);

				list = list.Where(hd =>
				{
					var booked = 0;
					if (hdIds.Contains(hd.Id) && bookedSums.TryGetValue(hd.Id, out var sum)) booked = sum;
					return booked < hd.RoomCount;
				}).ToList();
			}

			// load images
			var roomIds = list.Select(hd => hd.Room?.Id).Where(id => id > 0).Distinct().ToList();
			var images = await _db.Images
				.AsNoTracking()
				.Where(i => roomIds.Contains((int)i.RoomId))
				.ToListAsync();

			// services for rooms in list
			var svcDetails = await (from sd in _db.ServiceDetails.Where(sd => roomIds.Contains(sd.RoomId))
									join s in _db.Services on sd.ServiceId equals s.Id
									select new { sd.RoomId, ServiceName = s.ServiceName }).ToListAsync();

			var svcMap = svcDetails
				.GroupBy(x => x.RoomId)
				.ToDictionary(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

			// promotions for rooms in list (active)
			var now = DateTime.UtcNow.Date;
			var promoDetails = await _db.PromotionDetails
				.Include(pd => pd.Promotion)
				.Where(pd => roomIds.Contains(pd.RoomId) && pd.Promotion != null && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
				.ToListAsync();

			var promoMap = promoDetails
				.GroupBy(pd => pd.RoomId)
				.ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.DiscountPercent).ThenByDescending(x => x.Promotion.DiscountAmount).FirstOrDefault());

			var results = list.Select(hd => new ResultRoomDTO
			{
				Id = hd.Room?.Id ?? 0,
				RoomName = hd.Room?.RoomName ?? "Phòng",
				Status = hd.Status,
				Description = hd.Room?.Description ?? string.Empty,
				Size = hd.Room?.Size ?? 0,
				Price = hd.Room?.Price ?? 0,
				BedCount = hd.Room?.BedCount ?? 0,
				ImagePaths = (hd.Room != null)
					? images.Where(i => i.RoomId == hd.Room.Id).Select(i => i.ImagePath).ToList()
					: new List<string>(),
				HotelName = hd.Hotel?.HotelName,
				Services = svcMap.ContainsKey(hd.Room.Id) ? svcMap[hd.Room.Id] : new List<string>(),
				PromotionName = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.Promotion?.PromotionName : null,
				PromotionDiscountAmount = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.Promotion?.DiscountAmount : null,
				PromotionDiscountPercent = promoMap.ContainsKey(hd.Room.Id) ? promoMap[hd.Room.Id]?.DiscountPercent : null,
				RoomCount = hd.RoomCount,
				HotelDetailId = hd.Id
			}).ToList();

			return PartialView("~/Views/Booking/_AvailableRoomsPartial.cshtml", results);
		}

		// GET: /Booking/EditCustomer?id=123
		[HttpGet]
		public async Task<IActionResult> EditCustomer(int id)
		{
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
				return RedirectToAction("Index", "Login");

			var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
			if (booking == null)
				return NotFound();

			// Không cho phép sửa khi đã hoàn thành hoặc đã hủy
			if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
				return Forbid();

			var dto = new EditBookingCustomerDTO
			{
				Id = booking.Id,
				FullName = booking.FullName,
				Email = booking.Email,
				Phone = booking.Phone,
				AdditionalRequest = booking.AdditionalRequest
			};

			return View("EditCustomer", dto);
		}

		// POST: /Booking/EditCustomer
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> EditCustomer(EditBookingCustomerDTO dto)
		{
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
				return RedirectToAction("Index", "Login");

			if (!ModelState.IsValid)
			{
				return View("EditCustomer", dto);
			}

			var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == dto.Id && b.UserId == userId);
			if (booking == null)
				return NotFound();

			// Không cho phép sửa khi đã hoàn thành hoặc đã hủy
			if (booking.Status == BookingStatus.Cancelled || booking.Status == BookingStatus.Completed)
				return Forbid();

			booking.FullName = dto.FullName;
			booking.Email = dto.Email;
			booking.Phone = dto.Phone ?? string.Empty;
			booking.AdditionalRequest = dto.AdditionalRequest;

			await _db.SaveChangesAsync();

			TempData["SuccessMessage"] = "Cập nhật thông tin đặt phòng thành công.";
			return RedirectToAction("MyBookings");
		}
	}
}