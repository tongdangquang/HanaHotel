using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.GuestDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.Controllers
{
    public class AdminGuestController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public AdminGuestController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Guest");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultGuestDTO>>(jsonData);
                return View(values);
            }
            return View();
        }

        [HttpGet]
        public IActionResult AddGuest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddGuest(CreateGuestDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var jsonData = System.Text.Json.JsonSerializer.Serialize(model);
            var jsonContent = new StringContent(jsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiUrl}/api/Guest", jsonContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm khách mới.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpdateGuest(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Guest/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<UpdateGuestDTO>(jsonData);

                if (values != null)
                    return View(values);
                else
                    ModelState.AddModelError(string.Empty, "Không tìm thấy dữ liệu khách hàng.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lấy dữ liệu khách hàng.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateGuest(UpdateGuestDTO updateGuestViewModel)
        {
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(updateGuestViewModel);
            StringContent stringContent = new(jsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/api/Guest", stringContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật thông tin khách hàng.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DeleteGuest(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/Guest/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa khách hàng.");
            return View();
        }
    }
}
