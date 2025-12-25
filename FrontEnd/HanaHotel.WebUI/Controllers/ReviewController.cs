using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ReviewDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HanaHotel.WebUI.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public ReviewController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Review");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultReviewDTO>>(jsonData);
                return View(values ?? new List<ResultReviewDTO>());
            }

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lấy danh sách đánh giá.");
            return View(new List<ResultReviewDTO>());
        }

        [HttpGet]
        public IActionResult AddReview()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(AddReviewDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(model);
            var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiUrl}/api/Review", jsonContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi thêm đánh giá.");
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/Review/{id}");

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa đánh giá.");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> UpdateReview(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Review/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<UpdateReviewDTO>(jsonData);

                if (values != null)
                    return View(values);

                ModelState.AddModelError(string.Empty, "Không tìm thấy dữ liệu đánh giá.");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lấy dữ liệu đánh giá.");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReview(UpdateReviewDTO model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(model);
            var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/api/Review", stringContent);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi cập nhật đánh giá.");
            return View(model);
        }
    }
}
