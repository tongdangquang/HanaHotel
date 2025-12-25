using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.RoomDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;
using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using System.Linq;

namespace HanaHotel.WebUI.Controllers
{
    public class RoomController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly DataContext _db;

        public RoomController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings, DataContext db)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
            _db = db;
        }

        [AllowAnonymous]
        public async Task<IActionResult> Index(string? hotelName, string? roomName, DateTime? checkIn, DateTime? checkOut, string? bedCount, double? minSize, double? maxSize, decimal? maxPrice)
        {
            var hasFilter = !string.IsNullOrWhiteSpace(hotelName) ||
                            !string.IsNullOrWhiteSpace(roomName) ||
                            checkIn.HasValue || checkOut.HasValue ||
                            !string.IsNullOrWhiteSpace(bedCount) ||
                            minSize.HasValue || maxSize.HasValue ||
                            maxPrice.HasValue;

            // Always populate hotel list and room name list for the datalist dropdown in the view
            var hotels = await _db.Hotels
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

            ViewBag.Hotels = hotels;
            ViewBag.RoomNames = roomNames;

            if (!hasFilter)
            {
                // existing behavior: call API and render all rooms
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{_apiUrl}/api/Room");

                if (!response.IsSuccessStatusCode)
                    return NotFound();

                var jsonData = await response.Content.ReadAsStringAsync();
                var rooms = JsonConvert.DeserializeObject<List<ResultRoomDTO>>(jsonData);

                // If API didn't return ImagePaths, try to fill them (best-effort)
                if (rooms != null && rooms.Any())
                {
                    var roomIds = rooms.Select(r => r.Id).Distinct().ToList();
                    var images = await _db.Images
                        .AsNoTracking()
                        .Where(i => roomIds.Contains((int)i.RoomId))
                        .ToListAsync();

                    foreach (var r in rooms)
                    {
                        r.ImagePaths = images.Where(i => i.RoomId == r.Id).Select(i => i.ImagePath).ToList();

                    }
                    // --- Add Services ---
                    var svcMap = await _db.ServiceDetails
                        .Where(sd => roomIds.Contains(sd.RoomId))
                        .Join(_db.Services, sd => sd.ServiceId, s => s.Id, (sd, s) => new { sd.RoomId, s.ServiceName })
                        .GroupBy(x => x.RoomId)
                        .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

                    // --- Add Promotions ---
                    var nowDate1 = DateTime.UtcNow.Date;
                    var promoMap = await _db.PromotionDetails
                        .Include(pd => pd.Promotion)
                        .Where(pd => roomIds.Contains(pd.RoomId) && pd.Promotion.StartDate <= nowDate1 && pd.Promotion.EndDate >= nowDate1)
                        .GroupBy(pd => pd.RoomId)
                        .ToDictionaryAsync(g => g.Key, g => g.OrderByDescending(x => x.DiscountPercent)
                                                            .ThenByDescending(x => x.Promotion.DiscountAmount)
                                                            .FirstOrDefault());

                    foreach (var r in rooms)
                    {
                        if (svcMap.TryGetValue(r.Id, out var services))
                            r.Services = services;

                        if (promoMap.TryGetValue(r.Id, out var promo))
                        {
                            r.PromotionName = promo.Promotion?.PromotionName;
                            r.PromotionDiscountAmount = promo.Promotion?.DiscountAmount;
                            r.PromotionDiscountPercent = promo.DiscountPercent;
                        }
                    }

                }

                return View(rooms); // View nhận List<ResultRoomDTO>
            }

            // Search using local DB by joining HotelDetail, Hotel and Room
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

            // bedCount: user wants rooms with bedCount <= input
            if (!string.IsNullOrWhiteSpace(bedCount) && int.TryParse(bedCount.Trim(), out var bedCountInt))
            {
                hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.BedCount <= bedCountInt);
            }

            if (minSize.HasValue)
            {
                hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Size >= minSize.Value);
            }
            if (maxSize.HasValue)
            {
                hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Size <= maxSize.Value);
            }

            if (maxPrice.HasValue)
            {
                hdQuery = hdQuery.Where(hd => hd.Room != null && hd.Room.Price <= maxPrice.Value);
            }

            var list = await hdQuery.ToListAsync();

            // If date range provided, filter out hotelDetails which are fully booked in that range
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

            // Load images for the remaining rooms in one query to fill ImagePaths
            var roomIdsForResults = list.Select(hd => hd.Room?.Id ?? 0).Where(id => id > 0).Distinct().ToList();
            var imagesForResults = await _db.Images
                .AsNoTracking()
                .Where(i => roomIdsForResults.Contains((int)i.RoomId))
                .ToListAsync();

            var results = list.Select(hd => new ResultRoomDTO
            {
                Id = hd.Room?.Id ?? 0,
                RoomName = hd.Room?.RoomName ?? "Phòng",
                // Status moved to HotelDetail => use hd.Status
                Status = hd.Status,
                Description = hd.Room?.Description,
                Size = hd.Room?.Size ?? 0,
                Price = hd.Room?.Price ?? 0,
                BedCount = hd.Room?.BedCount ?? 0,
                ImagePaths = (hd.Room != null)
                    ? imagesForResults.Where(i => i.RoomId == hd.Room.Id).Select(i => i.ImagePath).ToList()
                    : new List<string>()
            }).ToList();


            // --- Thêm Services và Promotions cho tất cả rooms ---
            var roomIdsAll = results.Select(r => r.Id).ToList();

            // Services
            var svcMapAll = await _db.ServiceDetails
                .Where(sd => roomIdsAll.Contains(sd.RoomId))
                .Join(_db.Services, sd => sd.ServiceId, s => s.Id, (sd, s) => new { sd.RoomId, s.ServiceName })
                .GroupBy(x => x.RoomId)
                .ToDictionaryAsync(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

            // Promotions (active)
            var nowDate = DateTime.UtcNow.Date;
            var promoMapAll = await _db.PromotionDetails
                .Include(pd => pd.Promotion)
                .Where(pd => roomIdsAll.Contains(pd.RoomId) && pd.Promotion.StartDate <= nowDate && pd.Promotion.EndDate >= nowDate)
                .GroupBy(pd => pd.RoomId)
                .ToDictionaryAsync(g => g.Key, g => g.OrderByDescending(x => x.DiscountPercent)
                                                .ThenByDescending(x => x.Promotion.DiscountAmount)
                                                .FirstOrDefault());

            foreach (var room in results)
            {
                if (svcMapAll.TryGetValue(room.Id, out var services))
                    room.Services = services;

                if (promoMapAll.TryGetValue(room.Id, out var promo))
                {
                    room.PromotionName = promo.Promotion?.PromotionName;
                    room.PromotionDiscountAmount = promo.Promotion?.DiscountAmount;
                    room.PromotionDiscountPercent = promo.DiscountPercent;
                }
            }

            // If this is an AJAX request, return the partial grid HTML
            var isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjax)
            {
                return PartialView("_RoomGridPartial", results);
            }

            // Normal full-page request
            return View(results);
        }

        // Chi tiết phòng (giữ nguyên)
        [AllowAnonymous]
        public async Task<IActionResult> Detail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Room/{id}");

            if (!response.IsSuccessStatusCode)
                return NotFound();

            var jsonData = await response.Content.ReadAsStringAsync();
            var room = JsonConvert.DeserializeObject<ResultRoomDTO>(jsonData);

            // ensure images present
            if (room != null)
            {
                var images = await _db.Images.AsNoTracking().Where(i => i.RoomId == room.Id).Select(i => i.ImagePath).ToListAsync();
                room.ImagePaths = images;

                // Services
                room.Services = await (from sd in _db.ServiceDetails
                                       join s in _db.Services on sd.ServiceId equals s.Id
                                       where sd.RoomId == room.Id
                                       select s.ServiceName)
                                      .Distinct()
                                      .ToListAsync();

                // Promotion active tốt nhất
                var now = DateTime.UtcNow.Date;
                var promo = await _db.PromotionDetails
                    .Include(pd => pd.Promotion)
                    .Where(pd => pd.RoomId == room.Id && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
                    .OrderByDescending(pd => pd.DiscountPercent)
                    .ThenByDescending(pd => pd.Promotion.DiscountAmount)
                    .FirstOrDefaultAsync();

                if (promo != null)
                {
                    room.PromotionName = promo.Promotion?.PromotionName;
                    room.PromotionDiscountAmount = promo.Promotion?.DiscountAmount;
                    room.PromotionDiscountPercent = promo.DiscountPercent;
                }
            }

            return View(room);
        }
    }
}