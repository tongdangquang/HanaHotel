using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ReviewDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HotelProject.WebUI.ViewComponents.Default
{
    public class _ReviewPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _ReviewPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI; 
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Review");

            if (!responseMessage.IsSuccessStatusCode)
                return View("Error");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var reviewData = JsonConvert.DeserializeObject<List<ResultReviewDTO>>(jsonData);

            if (reviewData == null || reviewData.Count == 0)
                return View("Empty");

            return View(reviewData);
        }
    }
}
