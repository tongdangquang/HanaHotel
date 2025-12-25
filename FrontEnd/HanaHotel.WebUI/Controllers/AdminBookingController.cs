using System.Linq;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.BookingDTO;
using HanaHotel.WebUI.Models;
using HanaHotel.WebUI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminBookingController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl;

		public AdminBookingController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
			_apiUrl = appSettings.Value.urlAPI;
		}

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Booking");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultBookingDTO>>(jsonData);
                return View(values);
            }
            return View();
        }

        // helper: get hotel name + address either from hotelId or hotelDetailId (robust)
        private async Task<(string Name, string Address)> GetHotelInfoAsync(int? hotelId, int? hotelDetailId)
        {
            if ((hotelId == null || hotelId == 0) && (hotelDetailId == null || hotelDetailId == 0))
                return (string.Empty, string.Empty);

            try
            {
                var client = _httpClientFactory.CreateClient();

                // if we have hotelId, query /api/Hotel/{id}
                if (hotelId.HasValue && hotelId.Value > 0)
                {
                    var resp = await client.GetAsync($"{_apiUrl}/api/Hotel/{hotelId.Value}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var j = await resp.Content.ReadAsStringAsync();
                        try
                        {
                            var jo = JObject.Parse(j);
                            var name = jo.SelectToken("hotelName")?.ToString() ?? jo.SelectToken("HotelName")?.ToString() ?? jo.SelectToken("name")?.ToString() ?? string.Empty;
                            var addr = jo.SelectToken("address")?.ToString() ?? jo.SelectToken("Address")?.ToString() ?? string.Empty;
                            return (name ?? string.Empty, addr ?? string.Empty);
                        }
                        catch { }
                    }
                }

                // fallback: if we have hotelDetailId, try /api/HotelDetail/{id} then read HotelId and fetch hotel
                if (hotelDetailId.HasValue && hotelDetailId.Value > 0)
                {
                    var resp = await client.GetAsync($"{_apiUrl}/api/HotelDetail/{hotelDetailId.Value}");
                    if (resp.IsSuccessStatusCode)
                    {
                        var j = await resp.Content.ReadAsStringAsync();
                        try
                        {
                            var jo = JObject.Parse(j);
                            var hid = jo.SelectToken("hotelId")?.Value<int?>() ?? jo.SelectToken("HotelId")?.Value<int?>();
                            if (hid.HasValue && hid.Value > 0)
                            {
                                return await GetHotelInfoAsync(hid.Value, null);
                            }
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                // ignore and return empty
            }

            return (string.Empty, string.Empty);
        }

        // existing GetBookingDetails (improved to parse full ROOM_DETAIL list and to set hotel info)
        [HttpGet]
        public async Task<IActionResult> GetBookingDetails(int id)
        {
            if (id <= 0) return BadRequest();

            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Booking/{id}");
            if (!responseMessage.IsSuccessStatusCode)
                return StatusCode((int)responseMessage.StatusCode);

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonData))
                return NotFound();

            // try parse booking entity
            Booking? booking = null;
            try
            {
                booking = JsonConvert.DeserializeObject<Booking>(jsonData);
            }
            catch { booking = null; }

            JObject? bookingJo = null;
            try { bookingJo = JObject.Parse(jsonData); } catch { bookingJo = null; }

            var vm = new BookingRoomViewModel();

            // Basic mapping
            if (booking != null)
            {
                vm.BookingId = booking.Id;
                vm.FullName = booking.FullName ?? string.Empty;
                vm.BookingDate = booking.BookingDate;
                vm.CheckIn = booking.CheckInDate;
                vm.CheckOut = booking.CheckOutDate;
                vm.Email = booking.Email ?? string.Empty;
                vm.Phone = booking.Phone ?? string.Empty;
                vm.AdditionalRequest = booking.AdditionalRequest ?? string.Empty;
                vm.Status = booking.Status.ToString();
                vm.RoomId = booking.RoomId;
                vm.RoomName = await GetRoomNameAsync(booking.RoomId) ?? string.Empty;
            }
            else if (bookingJo != null)
            {
                vm.BookingId = bookingJo.SelectToken("id")?.Value<int>() ?? 0;
                DateTime dt;
                if (DateTime.TryParse(bookingJo.SelectToken("bookingDate")?.ToString(), out dt)) vm.BookingDate = dt;
                if (DateTime.TryParse(bookingJo.SelectToken("checkInDate")?.ToString(), out dt)) vm.CheckIn = dt;
                if (DateTime.TryParse(bookingJo.SelectToken("checkOutDate")?.ToString(), out dt)) vm.CheckOut = dt;
                vm.FullName = bookingJo.SelectToken("fullName")?.ToString()
                              ?? bookingJo.SelectToken("FullName")?.ToString()
                              ?? bookingJo.SelectToken("name")?.ToString()
                              ?? bookingJo.SelectToken("customerName")?.ToString()
                              ?? string.Empty;
                vm.Email = bookingJo.SelectToken("email")?.ToString() ?? string.Empty;
                vm.Phone = bookingJo.SelectToken("phone")?.ToString() ?? string.Empty;
                vm.AdditionalRequest = bookingJo.SelectToken("additionalRequest")?.ToString() ?? string.Empty;
                vm.RoomId = bookingJo.SelectToken("roomId")?.Value<int>() ?? 0;
                if (vm.RoomId > 0) vm.RoomName = await GetRoomNameAsync(vm.RoomId) ?? string.Empty;
                vm.Status = bookingJo.SelectToken("status")?.ToString() ?? string.Empty;
            }

            // compute nights
            var nights = 1;
            try { nights = Math.Max(1, (vm.CheckOut.Date - vm.CheckIn.Date).Days); } catch { nights = 1; }

            // extract rooms array if present (robust to many names / nested arrays)
            var roomDetails = new List<RoomDetailDto>();
            var discoveredHotels = new List<(int? HotelId, int? HotelDetailId, string Name, string Address)>();
            if (bookingJo != null)
            {
                // common names
                JToken? roomsToken = bookingJo.SelectToken("rooms")
                                    ?? bookingJo.SelectToken("bookingRooms")
                                    ?? bookingJo.SelectToken("roomsBooked")
                                    ?? bookingJo.SelectToken("roomDetails")
                                    ?? bookingJo.SelectToken("bookingRoomDetails")
                                    ?? bookingJo.SelectToken("room_detail")
                                    ?? bookingJo.SelectToken("roomDetail");

                // Fallback: search for any array that looks like a rooms array
                if (roomsToken == null)
                {
                    var arrays = bookingJo.Descendants().OfType<JArray>();
                    foreach (var arr in arrays)
                    {
                        var firstChild = arr.First as JObject;
                        if (firstChild == null) continue;

                        var hasRoomId = firstChild.SelectToken("roomId") != null || firstChild.SelectToken("RoomId") != null || firstChild.SelectToken("romId") != null || firstChild.SelectToken("id") != null;
                        if (!hasRoomId) continue;

                        // choose this array
                        roomsToken = arr;
                        break;
                    }
                }

                if (roomsToken != null && roomsToken.Type == JTokenType.Array)
                {
                    foreach (var r in roomsToken.Children())
                    {
                        var rid = r.SelectToken("roomId")?.Value<int>() ?? r.SelectToken("RoomId")?.Value<int>() ?? r.SelectToken("romId")?.Value<int>() ?? r.SelectToken("id")?.Value<int>() ?? 0;
                        var qty = r.SelectToken("quantity")?.Value<int>() ?? r.SelectToken("qty")?.Value<int>() ?? 1;
                        var adult = r.SelectToken("adultAmount")?.Value<int>() ?? r.SelectToken("adults")?.Value<int>() ?? 1;
                        var child = r.SelectToken("childrenAmount")?.Value<int>() ?? r.SelectToken("children")?.Value<int>() ?? 0;
                        decimal price = 0m;
                        var priceToken = r.SelectToken("price") ?? r.SelectToken("Price") ?? r.SelectToken("roomPrice");
                        if (priceToken != null) decimal.TryParse(priceToken.ToString(), out price);

                        // attempt to read hotel identifiers from the room-detail record
                        var hotelId = r.SelectToken("hotelId")?.Value<int?>() ?? r.SelectToken("HotelId")?.Value<int?>();
                        var hotelDetailId = r.SelectToken("hotelDetailId")?.Value<int?>() ?? r.SelectToken("HotelDetailId")?.Value<int?>();

                        // if price not present, try fetch from Room API
                        if (rid > 0 && price == 0m)
                        {
                            try
                            {
                                var roomResp = await client.GetAsync($"{_apiUrl}/api/Room/{rid}");
                                if (roomResp.IsSuccessStatusCode)
                                {
                                    var roomJson = await roomResp.Content.ReadAsStringAsync();
                                    var roomJo = JObject.Parse(roomJson);
                                    var pp = roomJo.SelectToken("price") ?? roomJo.SelectToken("Price") ?? roomJo.SelectToken("roomPrice");
                                    if (pp != null) decimal.TryParse(pp.ToString(), out price);
                                }
                            }
                            catch { }
                        }

                        // gather hotel info for display (cache later)
                        (string Name, string Address) hotelInfo = (string.Empty, string.Empty);
                        try
                        {
                            // call helper if we found a hotel id or detail id
                            if ((hotelId.HasValue && hotelId.Value > 0) || (hotelDetailId.HasValue && hotelDetailId.Value > 0))
                            {
                                hotelInfo = await GetHotelInfoAsync(hotelId, hotelDetailId);
                                discoveredHotels.Add((hotelId, hotelDetailId, hotelInfo.Name, hotelInfo.Address));
                            }
                        }
                        catch { }

                        var rd = new RoomDetailDto
                        {
                            RoomId = rid,
                            RoomName = (rid > 0) ? (await GetRoomNameAsync(rid) ?? string.Empty) : (r.SelectToken("roomName")?.ToString() ?? string.Empty),
                            Quantity = qty,
                            AdultAmount = adult,
                            ChildrenAmount = child,
                            Price = price
                        };
                        roomDetails.Add(rd);
                    }
                }
            }

            // fallback single room if none found
            if (!roomDetails.Any())
            {
                var price = 0m;
                if (vm.RoomId > 0)
                {
                    try
                    {
                        var roomResp = await client.GetAsync($"{_apiUrl}/api/Room/{vm.RoomId}");
                        if (roomResp.IsSuccessStatusCode)
                        {
                            var roomJson = await roomResp.Content.ReadAsStringAsync();
                            try
                            {
                                var roomJo = JObject.Parse(roomJson);
                                var pp = roomJo.SelectToken("price") ?? roomJo.SelectToken("Price") ?? roomJo.SelectToken("roomPrice");
                                if (pp != null) decimal.TryParse(pp.ToString(), out price);
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                var single = new RoomDetailDto
                {
                    RoomId = vm.RoomId,
                    RoomName = vm.RoomName,
                    Quantity = 1,
                    AdultAmount = 1,
                    ChildrenAmount = 0,
                    Price = price
                };
                roomDetails.Add(single);
            }

            vm.BookingRoomDetails = roomDetails;

            // compute subtotal and fill some fields from first room
            double subtotal = 0.0;
            foreach (var r in roomDetails)
            {
                var row = (double)(r.Price * r.Quantity * nights);
                subtotal += row;
            }
            vm.Subtotal = subtotal;

            var first = roomDetails.FirstOrDefault();
            if (first != null)
            {
                vm.Quantity = first.Quantity;
                vm.AdultAmount = first.AdultAmount;
                vm.ChildrenAmount = first.ChildrenAmount;
            }

            // try to get already-paid amount from booking JSON/entity
            decimal paid = 0m;
            // try common keys
            paid = TryGetDecimalFromJObject(bookingJo, "paidAmount", "amountPaid", "paid", "depositPaid", "deposit", "paid", "amount") ?? 0m;

            // fallback if Booking entity has a property (best-effort)
            try
            {
                if (booking != null)
                {
                    // check some properties dynamically
                    var jo = JObject.FromObject(booking);
                    paid = paid == 0m ? (TryGetDecimalFromJObject(jo, "paidAmount", "amountPaid", "deposit", "paid") ?? paid) : paid;
                }
            }
            catch { }

            vm.PaidAmount = paid;

            // compute DueAmount based on status rules:
            // Pending => due = total
            // Deposited => due = 1/2 * total
            // Paid => due = 0
            // others => 0
            decimal subtotalDec = (decimal)Math.Round(vm.Subtotal, 0);
            var statusNorm = (vm.Status ?? string.Empty).Trim().ToLowerInvariant();
            decimal due = 0m;
            if (statusNorm == "pending")
            {
                due = subtotalDec;
            }
            else if (statusNorm == "deposited")
            {
                due = subtotalDec / 2m;
            }
            else if (statusNorm == "paid")
            {
                due = 0m;
            }
            else
            {
                due = 0m;
            }

            vm.DueAmount = Math.Max(0m, due);

            // set hotel info on VM:
            // - if only one distinct hotel discovered, show its name/address
            // - if multiple, join names and leave address blank (or you can adapt to your UX)
            var distinctHotels = discoveredHotels
                                    .Where(x => !string.IsNullOrWhiteSpace(x.Name))
                                    .Select(x => (x.Name, x.Address))
                                    .Distinct()
                                    .ToList();

            if (distinctHotels.Count == 1)
            {
                vm.HotelName = distinctHotels[0].Name;
                vm.HotelAddress = distinctHotels[0].Address;
            }
            else if (distinctHotels.Count > 1)
            {
                vm.HotelName = string.Join(" / ", distinctHotels.Select(h => h.Name));
                vm.HotelAddress = string.Join(" / ", distinctHotels.Select(h => h.Address).Where(a => !string.IsNullOrWhiteSpace(a)));
            }
            else
            {
                // try to infer hotel from first room via Room -> HotelDetail -> Hotel if available
                if (roomDetails.FirstOrDefault()?.RoomId is int someRid && someRid > 0)
                {
                    try
                    {
                        // try to get room's hotel by hitting Room detail (best-effort)
                        var respRoom = await client.GetAsync($"{_apiUrl}/api/Room/{someRid}");
                        if (respRoom.IsSuccessStatusCode)
                        {
                            var rj = await respRoom.Content.ReadAsStringAsync();
                            try
                            {
                                var rjo = JObject.Parse(rj);
                                var hotelIdFromRoom = rjo.SelectToken("hotelId")?.Value<int?>() ?? rjo.SelectToken("HotelId")?.Value<int?>();
                                if (hotelIdFromRoom.HasValue && hotelIdFromRoom.Value > 0)
                                {
                                    var h = await GetHotelInfoAsync(hotelIdFromRoom.Value, null);
                                    vm.HotelName = h.Name;
                                    vm.HotelAddress = h.Address;
                                }
                            }
                            catch { }
                        }
                    }
                    catch { }
                }
            }

            return PartialView("_BookingDetailsPartial", vm);
        }

        // New Details action: reuse GetBookingDetails logic then return full view
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            if (id <= 0) return BadRequest();

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Booking/{id}");
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(json)) return NotFound();

            // parse root object: { booking: {...}, roomDetails: [ ... ] }
            JObject root = null!;
            try { root = JObject.Parse(json); } catch { return StatusCode(500, "Invalid JSON from API"); }

            var vm = new BookingRoomViewModel();

            // booking node (may be plain object or some other naming)
            var bookingToken = root["booking"] ?? root;
            if (bookingToken != null)
            {
                vm.BookingId = bookingToken.Value<int?>("id") ?? bookingToken.Value<int?>("bookingId") ?? 0;
                vm.FullName = bookingToken.Value<string>("fullName") ?? bookingToken.Value<string>("FullName") ?? bookingToken.Value<string>("name") ?? string.Empty;
                vm.Email = bookingToken.Value<string>("email") ?? string.Empty;
                vm.Phone = bookingToken.Value<string>("phone") ?? string.Empty;
                vm.AdditionalRequest = bookingToken.Value<string>("additionalRequest") ?? string.Empty;
                vm.Status = bookingToken.Value<string>("status") ?? bookingToken.Value<string>("Status") ?? string.Empty;

                // dates
                if (DateTime.TryParse(bookingToken.Value<string>("bookingDate") ?? bookingToken.Value<string>("BookingDate"), out var bd)) vm.BookingDate = bd;
                if (DateTime.TryParse(bookingToken.Value<string>("checkInDate") ?? bookingToken.Value<string>("CheckInDate"), out var ci)) vm.CheckIn = ci;
                if (DateTime.TryParse(bookingToken.Value<string>("checkOutDate") ?? bookingToken.Value<string>("CheckOutDate"), out var co)) vm.CheckOut = co;
            }

            // nights
            var nights = 1;
            try { nights = Math.Max(1, (vm.CheckOut.Date - vm.CheckIn.Date).Days); } catch { nights = 1; }

            // parse roomDetails array
            var roomDetailsToken = root["roomDetails"] ?? root["rooms"] ?? root["bookingRooms"] ?? root["roomDetailsList"];
            var details = new List<RoomDetailDto>();
            if (roomDetailsToken != null && roomDetailsToken.Type == JTokenType.Array)
            {
                foreach (var r in roomDetailsToken.Children())
                {
                    var rid = r.Value<int?>("roomId") ?? r.Value<int?>("roomId") ?? r.Value<int?>("roomId") ?? r.Value<int?>("roomId") ?? r.Value<int?>("roomId") ?? 0;
                    // more robust getters:
                    rid = r.SelectToken("roomId")?.Value<int>() ?? r.SelectToken("RoomId")?.Value<int>() ?? r.SelectToken("roomId")?.Value<int>() ?? r.SelectToken("roomId")?.Value<int>() ?? rid;

                    var qty = r.Value<int?>("quantity") ?? r.Value<int?>("qty") ?? 1;
                    var adult = r.Value<int?>("adultAmount") ?? r.Value<int?>("adults") ?? 1;
                    var child = r.Value<int?>("childrenAmount") ?? r.Value<int?>("children") ?? 0;
                    var price = r.Value<decimal?>("price") ?? r.Value<decimal?>("Price") ?? 0m;

                    var rd = new RoomDetailDto
                    {
                        RoomId = rid,
                        RoomName = r.Value<string>("roomName") ?? r.Value<string>("RoomName") ?? string.Empty,
                        Quantity = qty,
                        AdultAmount = adult,
                        ChildrenAmount = child,
                        Price = price
                    };

                    // capture hotel info if provided
                    var hotelName = r.Value<string>("hotelName") ?? r.Value<string>("HotelName");
                    var hotelAddress = r.Value<string>("hotelAddress") ?? r.Value<string>("HotelAddress");
                    if (!string.IsNullOrWhiteSpace(hotelName) && string.IsNullOrWhiteSpace(vm.HotelName))
                    {
                        vm.HotelName = hotelName;
                        vm.HotelAddress = hotelAddress ?? string.Empty;
                    }

                    details.Add(rd);
                }
            }
            else
            {
                // fallback: try to read old-style booking.roomId + fetch room info
                var directRoomId = bookingToken?.Value<int?>("roomId") ?? bookingToken?.Value<int?>("RoomId");
                if (directRoomId.HasValue && directRoomId.Value > 0)
                {
                    // try get room info
                    try
                    {
                        var roomResp = await client.GetAsync($"{_apiUrl}/api/Room/{directRoomId.Value}");
                        if (roomResp.IsSuccessStatusCode)
                        {
                            var roomJson = await roomResp.Content.ReadAsStringAsync();
                            var roomJo = JObject.Parse(roomJson);
                            var roomName = roomJo.Value<string>("roomName") ?? roomJo.Value<string>("RoomName") ?? roomJo.Value<string>("name") ?? string.Empty;
                            var price = roomJo.Value<decimal?>("price") ?? 0m;
                            details.Add(new RoomDetailDto
                            {
                                RoomId = directRoomId.Value,
                                RoomName = roomName ?? string.Empty,
                                Quantity = 1,
                                AdultAmount = 1,
                                ChildrenAmount = 0,
                                Price = price
                            });
                        }
                    }
                    catch { }
                }
            }

            vm.BookingRoomDetails = details;

            // compute subtotal
            decimal subtotal = 0m;
            foreach (var rd in vm.BookingRoomDetails)
            {
                subtotal += rd.Price * rd.Quantity * (decimal)nights;
            }
            vm.Subtotal = (double)subtotal;

            // paid amount: try find in bookingToken
            decimal paid = 0m;
            decimal.TryParse(bookingToken?.Value<string>("paidAmount") ?? bookingToken?.Value<string>("amountPaid") ?? bookingToken?.Value<string>("paid") ?? "0", out paid);
            vm.PaidAmount = paid;

            // due according to rules
            var status = (vm.Status ?? string.Empty).Trim().ToLowerInvariant();
            decimal due = 0m;
            if (status == "pending") due = subtotal;
            else if (status == "deposited") due = subtotal / 2m;
            else due = 0m;
            vm.DueAmount = Math.Max(0m, due);

            return View("Details", vm);
        }

        // helper to query room name from API (robust to several JSON property namings)
        private async Task<string?> GetRoomNameAsync(int roomId)
        {
            if (roomId <= 0) return null;
            try
            {
                var client = _httpClientFactory.CreateClient();
                var resp = await client.GetAsync($"{_apiUrl}/api/Room/{roomId}");
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(json)) return null;

                try
                {
                    var jo = JObject.Parse(json);
                    // try common fields
                    var name = jo.Value<string>("roomName") ?? jo.Value<string>("RoomName") ?? jo.Value<string>("name") ?? jo.Value<string>("Name");
                    if (!string.IsNullOrWhiteSpace(name)) return name;
                }
                catch
                {
                    // if API returns an array or different shape, try deserialize to dynamic
                    dynamic? dyn = JsonConvert.DeserializeObject<dynamic>(json);
                    if (dyn != null)
                    {
                        try
                        {
                            string? n = dyn.roomName ?? dyn.RoomName ?? dyn.name ?? dyn.Name;
                            if (!string.IsNullOrWhiteSpace(n)) return n;
                        }
                        catch { }
                    }
                }
            }
            catch
            {
                // ignore and return null
            }
            return null;
        }

        // helper to try extract decimal-like value from JObject using many possible property names
        private decimal? TryGetDecimalFromJObject(JObject? jo, params string[] keys)
        {
            if (jo == null) return null;
            foreach (var k in keys)
            {
                try
                {
                    var token = jo.SelectToken(k) ?? jo.Property(k)?.Value;
                    if (token != null)
                    {
                        if (token.Type == JTokenType.Float || token.Type == JTokenType.Integer)
                        {
                            return token.Value<decimal>();
                        }

                        var s = token.ToString();
                        if (decimal.TryParse(s, out var d))
                            return d;
                    }
                }
                catch { continue; }
            }
            // also try direct common paths
            foreach (var prop in jo.Properties())
            {
                var nameLower = prop.Name.ToLowerInvariant();
                if (nameLower.Contains("paid") || nameLower.Contains("amount") || nameLower.Contains("total") || nameLower.Contains("price") || nameLower.Contains("deposit"))
                {
                    if (decimal.TryParse(prop.Value.ToString(), out var d2))
                        return d2;
                }
            }
            return null;
        }

        [HttpGet]
        public async Task<IActionResult> UpdateBooking(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Booking/{id}");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();

                // Lấy đầy đủ entity Booking từ API để hiển thị thông tin chi tiết
                var booking = JsonConvert.DeserializeObject<Booking>(jsonData);

                if (booking == null)
                    return NotFound();

                // Map sang DTO để bind form cập nhật
                var updateDto = new UpdateBookingDTO
                {
                    Id = booking.Id,
                    CheckInDate = booking.CheckInDate,
                    CheckOutDate = booking.CheckOutDate,
                    Status = booking.Status,
                    AdditionalRequest = booking.AdditionalRequest,
                    RoomId = booking.RoomId,
                    UserId = booking.UserId
                };

                // Truyền đối tượng booking đầy đủ cho view (chỉ để hiển thị thông tin)
                ViewBag.FullBooking = booking;

                // Lấy tên phòng để hiển thị thay vì ID
                var roomName = await GetRoomNameAsync(booking.RoomId);
                ViewBag.RoomName = roomName;

                return View(updateDto);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateBooking(UpdateBookingDTO updateBookingDTO)
        {
            var client = _httpClientFactory.CreateClient();

            var existingBookingResponse = await client.GetAsync($"{_apiUrl}/api/Booking/{updateBookingDTO.Id}");

            if (!existingBookingResponse.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Không thể lấy thông tin chi tiết của đơn đặt phòng.");
                return View(updateBookingDTO);
            }

            var existingBookingJson = await existingBookingResponse.Content.ReadAsStringAsync();
            var existingBooking = JsonConvert.DeserializeObject<Booking>(existingBookingJson);

            if (existingBooking == null)
            {
                ModelState.AddModelError("", "Đơn đặt phòng không tồn tại.");
                return View(updateBookingDTO);
            }

            // Nếu cập nhật thất bại và phải hiển thị lại view, cần truyền lại thông tin chi tiết
            ViewBag.FullBooking = existingBooking;

            // Lấy tên phòng để hiển thị lại khi render view (nếu cần)
            ViewBag.RoomName = await GetRoomNameAsync(existingBooking.RoomId);

            existingBooking.CheckInDate = updateBookingDTO.CheckInDate;
            existingBooking.CheckOutDate = updateBookingDTO.CheckOutDate;
            existingBooking.Status = updateBookingDTO.Status;
            existingBooking.AdditionalRequest = updateBookingDTO.AdditionalRequest ?? existingBooking.AdditionalRequest;

            // Gửi entity đã cập nhật về API
            var updatedContent = new StringContent(JsonConvert.SerializeObject(existingBooking), System.Text.Encoding.UTF8, "application/json");
            var updateResponse = await client.PutAsync($"{_apiUrl}/api/Booking", updatedContent);

            if (updateResponse.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Không thể cập nhật đơn đặt phòng. Vui lòng thử lại.");
            return View(updateBookingDTO);
        }


        [HttpGet]
        public async Task<IActionResult> DeleteBooking(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/Booking/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa đơn đặt phòng.");
            return View();
        }
    }

}