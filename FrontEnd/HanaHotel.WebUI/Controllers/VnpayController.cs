using HanaHotel.DataAccessLayer.Concrete;
using HanaHotel.EntityLayer.Concrete;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace HanaHotel.WebUI.Controllers.Api
{
	[ApiController]
	[Route("api/[controller]")]
	public class VnpayController : ControllerBase
	{
		private readonly DataContext _db;
		private readonly ILogger<VnpayController> _logger;
		private readonly string _vnpaySecret;

		public VnpayController(DataContext db, IConfiguration cfg, ILogger<VnpayController> logger)
		{
			_db = db;
			_logger = logger;
			_vnpaySecret = cfg["VNPAY:HashSecret"] ?? string.Empty;
		}

		// Accept GET or POST depending on VNPAY configuration
		[HttpGet("Callback")]
		[HttpPost("Callback")]
		public async Task<IActionResult> Callback()
		{
			var q = Request.Query;

			// validate signature if secret available
			if (string.IsNullOrEmpty(_vnpaySecret))
			{
				_logger.LogWarning("Vnpay callback: secret key not configured.");
				return BadRequest("Missing secret");
			}

			// Build redirect URL to PaymentController.VnPayReturn preserving querystring
			var returnPath = Url.Action("VnPayReturn", "Payment") ?? "/Payment/VnPayReturn";
			var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
			var redirectUrl = returnPath + qs;

			_logger.LogInformation("Vnpay callback: redirecting to {RedirectUrl}", redirectUrl);

			// Issue 302 redirect so browser lands on Payment/VnPayReturn with original query params
			return Redirect(redirectUrl);
		}
	}
}