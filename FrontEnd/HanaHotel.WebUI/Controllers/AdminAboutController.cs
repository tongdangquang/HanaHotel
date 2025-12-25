using HanaHotel.WebUI.DTOs.AboutDTO;
using HanaHotel.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Newtonsoft.Json;
using System.Data;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminAboutController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl;

		public AdminAboutController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
			_apiUrl = appSettings.Value.urlAPI;
		}
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/About");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultAboutDTO>>(jsonData);

                if (values == null || values.Count == 0)
                    return View(null);

                return View(values[0]);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(ResultAboutDTO updateAboutDTO)
        {
            // Dinamik olarak sayıları al
            using (var connection = new MySqlConnection("server=localhost;user=root;password=root;database=HanaHoteldb"))
            {
                var command = new MySqlCommand("GetCounts", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                await connection.OpenAsync();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        updateAboutDTO.StaffCount = reader.GetInt32(0);
                        updateAboutDTO.RoomCount = reader.GetInt32(1);
                        updateAboutDTO.CustomerCount = reader.GetInt32(2);
                    }
                }
            }

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateAboutDTO);
            StringContent stringContent = new(jsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/api/About", stringContent);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", new { id = updateAboutDTO.AboutId });
            }

            // PUT isteği başarısızsa, hata mesajı ekle
            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin giới thiệu.");
            return View(updateAboutDTO);
        }


    }
}


