using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.DTOs.PromotionDetailDTO;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.Controllers
{
    public class PromotionDetailController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public PromotionDetailController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/PromotionDetail");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultPromotionDetailDTO>>(json);
                return View(values);
            }

            return View();
        }

        [HttpGet]
        public IActionResult AddPromotionDetail()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPromotionDetail(CreatePromotionDetailDTO model)
        {
            if (!ModelState.IsValid) return View();

            var client = _httpClientFactory.CreateClient();
            var json = System.Text.Json.JsonSerializer.Serialize(model);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiUrl}/api/PromotionDetail", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Không thể thêm chi tiết khuyến mãi.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> DeletePromotionDetail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/PromotionDetail/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Không thể xóa chi tiết khuyến mãi.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> UpdatePromotionDetail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/PromotionDetail/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var value = JsonConvert.DeserializeObject<UpdatePromotionDetailDTO>(json);
                return View(value);
            }

            ModelState.AddModelError("", "Không thể lấy dữ liệu chi tiết khuyến mãi.");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePromotionDetail(UpdatePromotionDetailDTO model)
        {
            if (!ModelState.IsValid)
                return View();

            var client = _httpClientFactory.CreateClient();
            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/api/PromotionDetail", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Không thể cập nhật chi tiết khuyến mãi.");
            return View();
        }
    }
}
