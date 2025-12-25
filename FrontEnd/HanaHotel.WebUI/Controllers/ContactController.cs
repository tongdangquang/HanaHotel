using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ContactDTO;
using HanaHotel.WebUI.DTOs.MessageCategoryDTO;
using System.Text;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.Controllers
{
    [AllowAnonymous]

    public class ContactController : Controller

    {
        private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl;

		public ContactController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/MessageCategory");

            var jsonData = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            var categories = JsonConvert.DeserializeObject<List<ResultMessageCategoryDTO>>(jsonData);

            List<SelectListItem> values = categories!.Select(item => new SelectListItem
            {
                Text = item.MessageCategoryName,
                Value = item.MessageCategoryId.ToString()
            }).ToList();

            ViewBag.Categories = values;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(CreateContactDTO contactDTO)
        {
            if (!ModelState.IsValid || !await PostContact(contactDTO))
            {
                ViewBag.Categories = await GetMessageCategories();
                if (!ModelState.IsValid)
                    return View(contactDTO);

                ModelState.AddModelError("", "Đã xảy ra lỗi khi gửi biểu mẫu liên hệ.");
                return View(contactDTO);
            }

            return RedirectToAction("Index", "Default");
        }

        private async Task<bool> PostContact(CreateContactDTO contactDTO)
        {
            contactDTO.Date = DateTime.Now;
            var client = _httpClientFactory.CreateClient();
            var jsonData = JsonConvert.SerializeObject(contactDTO);
            var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_apiUrl}/api/Contact", content);
            return response.IsSuccessStatusCode;
        }

        private async Task<List<SelectListItem>> GetMessageCategories()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/MessageCategory");
            var jsonData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var categories = JsonConvert.DeserializeObject<List<ResultMessageCategoryDTO>>(jsonData);

            return categories!.Select(item => new SelectListItem
            {
                Text = item.MessageCategoryName,
                Value = item.MessageCategoryId.ToString()
            }).ToList();
        }
    }
}

