using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using VNPAY;
using VNPAY.Models;
using VNPAY.Models.Enums;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize]
	public partial class PaymentController : Controller
	{
		private readonly DataContext _db;
		private readonly IVnpayClient _vnpayClient;
		private readonly ILogger<PaymentController> _logger;
		private readonly IHttpClientFactory _httpFactory;
		private readonly string _vnpaySecret;

		// extra charge policy
		private const int MAX_FREE_GUESTS = 3;
		private const decimal EXTRA_CHARGE_PER_PERSON = 100000m;

		public PaymentController(DataContext db, IConfiguration cfg, ILogger<PaymentController> logger, IHttpClientFactory httpFactory, IVnpayClient vnpayClient)
		{
			_db = db;
			_logger = logger;
			_httpFactory = httpFactory;
			_vnpayClient = vnpayClient;
			_vnpaySecret = cfg["VNPAY:HashSecret"] ?? string.Empty;
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> CreateVNPay(int bookingId, string paymentOption)
		{
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
			{
				_logger.LogWarning("CreateVNPay: user not authenticated.");
				return BadRequest(new { success = false, message = "Bạn cần đăng nhập." });
			}

			var booking = await _db.Bookings.AsNoTracking().FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);
			if (booking == null)
			{
				_logger.LogWarning("CreateVNPay: booking {BookingId} not found for user {UserId}.", bookingId, userId);
				return BadRequest(new { success = false, message = "Đơn không tồn tại." });
			}

			var roomDetails = await _db.RoomDetails.Where(rd => rd.BookingId == bookingId).ToListAsync();
			var roomIds = roomDetails.Select(rd => rd.RoomId).Distinct().ToList();
			var rooms = await _db.Rooms.Where(r => roomIds.Contains(r.Id)).ToDictionaryAsync(r => r.Id, r => r);
			var nights = Math.Max(1, (int)(booking.CheckOutDate.Date - booking.CheckInDate.Date).TotalDays);

			decimal total = 0m;
			foreach (var rd in roomDetails)
			{
				rooms.TryGetValue(rd.RoomId, out var room);
				var price = room != null ? (decimal)room.Price : 0m;

				// base price (apply later the same promotion logic if required; here we keep base)
				total += price * rd.Quantity * nights;

				// extra charge for extra adults beyond MAX_FREE_GUESTS
				var extraPersons = Math.Max(0, rd.AdultAmount - MAX_FREE_GUESTS);
				if (extraPersons > 0)
				{
					total += extraPersons * EXTRA_CHARGE_PER_PERSON * rd.Quantity * nights;
				}
			}

			var amount = paymentOption?.Equals("deposit", StringComparison.OrdinalIgnoreCase) == true
				? Math.Round(total * 0.5m, 0)
				: Math.Round(total, 0);

			if (amount <= 0m)
			{
				_logger.LogWarning("CreateVNPay: computed amount is zero or negative for booking {BookingId}. Total={Total}", bookingId, total);
				return BadRequest(new { success = false, message = "Số tiền thanh toán không hợp lệ." });
			}

			var request = new VnpayPaymentRequest
			{
				Money = (double)amount,
				Description = $"Thanh toán đặt phòng #{bookingId} ({(paymentOption == "deposit" ? "Đặt cọc" : "Thanh toán toàn bộ")})",
				BankCode = BankCode.ANY,
				Language = DisplayLanguage.Vietnamese
			};

			var paymentUrlInfor = _vnpayClient.CreatePaymentUrl(request);
			var paymentUrl = paymentUrlInfor?.Url;

			if (string.IsNullOrEmpty(paymentUrl))
			{
				_logger.LogError("CreateVNPay: VNPAY returned empty URL for booking {BookingId}. Response: {@Response}", bookingId, paymentUrlInfor);
				return StatusCode(502, new { success = false, message = "Lỗi tạo đường dẫn thanh toán." });
			}

			return Ok(new { success = true, url = paymentUrl });
		}

		[AllowAnonymous]
		[HttpGet]
		public async Task<IActionResult> VnPayReturn()
		{
			var q = Request.Query;
			var txnRef = q["vnp_TxnRef"].ToString();
			var orderInfo = q["vnp_OrderInfo"].ToString();
			var secureHash = q["vnp_SecureHash"].ToString();
			var respCode = q["vnp_ResponseCode"].ToString();
			var detectedPaymentType = string.Empty;
			// legacy fallback
			if (string.IsNullOrEmpty(respCode) && q.TryGetValue("errorCode", out var ec)) respCode = ec.ToString();

			int bookingId = 0;
			try
			{
				// 1) Try parse pattern like "order-<id>" or "txn-<id>"
				if (!string.IsNullOrEmpty(txnRef))
				{
					var parts = txnRef.Split('-', StringSplitOptions.RemoveEmptyEntries);
					if (parts.Length >= 2 && int.TryParse(parts[1], out var idFromTxn))
					{
						bookingId = idFromTxn;
					}
					else
					{
						// if txnRef itself is numeric and represents booking id
						if (int.TryParse(txnRef, out var numericTxn) && numericTxn > 0 && numericTxn < 1_000_000_000)
						{
							bookingId = numericTxn;
						}
					}
				}

				// 2) If not found, try to extract "#<digits>" from orderInfo (description)
				if (bookingId == 0 && !string.IsNullOrEmpty(orderInfo))
				{
					var m = Regex.Match(orderInfo, @"#(?<id>\d+)");
					if (m.Success && int.TryParse(m.Groups["id"].Value, out var idFromOrderInfo))
					{
						bookingId = idFromOrderInfo;
					}
				}

				// 3) Detect payment option from orderInfo text (created in CreateVNPay)
				if (!string.IsNullOrEmpty(orderInfo))
				{
					if (orderInfo.Contains("Đặt cọc", StringComparison.OrdinalIgnoreCase) ||
						orderInfo.Contains("đặt cọc", StringComparison.OrdinalIgnoreCase) ||
						orderInfo.Contains("deposited", StringComparison.OrdinalIgnoreCase))
					{
						detectedPaymentType = "deposited";
					}
					else if (orderInfo.Contains("Thanh toán toàn bộ", StringComparison.OrdinalIgnoreCase) ||
							 orderInfo.Contains("Thanh toán toàn bộ", StringComparison.InvariantCultureIgnoreCase) ||
							 orderInfo.Contains("paid", StringComparison.OrdinalIgnoreCase))
					{
						detectedPaymentType = "paid";
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "VnPayReturn: error parsing booking id / payment type from vnp_TxnRef or vnp_OrderInfo. txnRef={TxnRef} orderInfo={OrderInfo}", txnRef, orderInfo);
			}

			// VNPAY success code is "00" (some libs use "0") — accept both
			var success = respCode == "00" || respCode == "0";

			if (success && bookingId > 0)
			{
				var booking = await _db.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
				if (booking != null)
				{
					var previousStatus = booking.Status;

					if (detectedPaymentType == "deposited")
					{
						booking.Status = BookingStatus.Deposited;
						_logger.LogInformation("VnPayReturn: booking {BookingId} updated to Deposited (deposit payment).", bookingId);
					}
					else
					{
						booking.Status = BookingStatus.Paid;
						_logger.LogInformation("VnPayReturn: booking {BookingId} updated to Paid (full payment or unspecified).", bookingId);
					}

					if (previousStatus != BookingStatus.Paid && previousStatus != BookingStatus.Deposited)
					{
						var roomDetails = await _db.RoomDetails.Where(rd => rd.BookingId == bookingId).ToListAsync();
						var hotelDetailIds = roomDetails.Select(rd => rd.HotelDetailId).Distinct().ToList();

						var hotelDetails = await _db.HotelDetails.Where(x => hotelDetailIds.Contains(x.Id)).ToListAsync();
						foreach (var hd in hotelDetails)
						{
							var totalBookedRooms = roomDetails.Where(rd => rd.HotelDetailId == hd.Id).Sum(rd => rd.Quantity);
							hd.RoomCount = Math.Max(0, hd.RoomCount - totalBookedRooms);
						}
					}

					_db.Bookings.Update(booking);
					await _db.SaveChangesAsync();
				}
				else
				{
					_logger.LogWarning("VnPayReturn: booking {BookingId} not found while processing success return.", bookingId);
				}
			}
			else if (bookingId == 0)
			{
				_logger.LogWarning("VnPayReturn: could not determine bookingId. txnRef={TxnRef} orderInfo={OrderInfo}", txnRef, orderInfo);
			}

			ViewBag.Message = success ? "Thanh toán VNPAY thành công." : $"Thanh toán thất bại. Mã: {respCode}";
			ViewBag.Redirect = Url.Action("MyBookings", "Booking");
			return View("Result");
		}

		// GET: /Payment/Index
		[HttpGet]
		public async Task<IActionResult> Index(int bookingId)
		{
			var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
			if (!int.TryParse(idClaim, out var userId))
			{
				return RedirectToAction("Index", "Login");
			}

			var booking = await _db.Bookings
				.AsNoTracking()
				.FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

			if (booking == null)
				return NotFound();

			var roomDetails = await _db.RoomDetails
				.Where(rd => rd.BookingId == bookingId)
				.ToListAsync();

			var roomIds = roomDetails.Select(rd => rd.RoomId).Distinct().ToList();
			var rooms = await _db.Rooms
				.Where(r => roomIds.Contains(r.Id))
				.ToDictionaryAsync(r => r.Id, r => r);

			var nights = Math.Max(1, (int)(booking.CheckOutDate.Date - booking.CheckInDate.Date).TotalDays);

			// load services for rooms
			var svcDetails = await (from sd in _db.ServiceDetails.Where(sd => roomIds.Contains(sd.RoomId))
									join s in _db.Services on sd.ServiceId equals s.Id
									select new { sd.RoomId, ServiceName = s.ServiceName }).ToListAsync();

			var svcMap = svcDetails
				.GroupBy(x => x.RoomId)
				.ToDictionary(g => g.Key, g => g.Select(x => x.ServiceName).Distinct().ToList());

			// load active promotions for rooms (choose best)
			var now = DateTime.UtcNow.Date;
			var promoDetails = await _db.PromotionDetails
				.Include(pd => pd.Promotion)
				.Where(pd => roomIds.Contains(pd.RoomId) && pd.Promotion != null && pd.Promotion.StartDate <= now && pd.Promotion.EndDate >= now)
				.ToListAsync();

			var promoMap = promoDetails
				.GroupBy(pd => pd.RoomId)
				.ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.DiscountPercent).ThenByDescending(x => x.Promotion.DiscountAmount).FirstOrDefault());

			decimal total = 0m;
			var details = new List<PaymentRoomDetail>();
			foreach (var rd in roomDetails)
			{
				rooms.TryGetValue(rd.RoomId, out var room);
				var price = room != null ? (decimal)room.Price : 0m;

				// find promotion if any
				promoMap.TryGetValue(rd.RoomId, out var pd);
				double pct = pd?.DiscountPercent != null ? (double)pd?.DiscountPercent : 0;
				decimal amt = pd?.Promotion?.DiscountAmount ?? 0m;

				// compute effective unit price
				decimal effective = price;
				if (pct > 0)
				{
					effective = price * (1 - (decimal)(pct / 100.0));
				}
				else if (amt > 0)
				{
					effective = Math.Max(0m, price - amt);
				}

				var subtotal = effective * rd.Quantity * nights;

				// extra charge for extra adults beyond MAX_FREE_GUESTS
				var extraPersons = Math.Max(0, rd.AdultAmount - MAX_FREE_GUESTS);
				decimal extraCharge = 0m;
				if (extraPersons > 0)
				{
					extraCharge = extraPersons * EXTRA_CHARGE_PER_PERSON * rd.Quantity * nights;
					subtotal += extraCharge;
				}

				total += subtotal;

				var det = new PaymentRoomDetail
				{
					RoomId = rd.RoomId,
					RoomName = room?.RoomName ?? "Phòng",
					Quantity = rd.Quantity,
					Price = price,
					EffectiveUnitPrice = effective,
					Subtotal = subtotal,
					Services = svcMap.ContainsKey(rd.RoomId) ? svcMap[rd.RoomId] : new List<string>(),
					PromotionName = pd?.Promotion?.PromotionName,
					PromotionDiscountPercent = pd?.DiscountPercent != null ? (double)pd?.DiscountPercent : 0,
					PromotionDiscountAmount = pd?.Promotion?.DiscountAmount
				};

				// if the model has an ExtraCharge field you can set it here; otherwise subtotal already includes it
				// det.ExtraCharge = extraCharge;

				details.Add(det);
			}

			var model = new PaymentViewModel
			{
				BookingId = booking.Id,
				FullName = booking.FullName,
				Email = booking.Email,
				Phone = booking.Phone,
				CheckIn = booking.CheckInDate,
				CheckOut = booking.CheckOutDate,
				Nights = nights,
				TotalAmount = total,
				DepositAmount = Math.Round(total * 0.5m, 0),
				RoomDetails = details
			};

			return View(model);
		}
	}
}