using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.AboutDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HotelProject.WebUI.ViewComponents.Default
{
    public class _AboutUsPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _AboutUsPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/About");

            if (!responseMessage.IsSuccessStatusCode)
                return View("Error");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var aboutData = JsonConvert.DeserializeObject<List<ResultAboutDTO>>(jsonData);

            if (aboutData == null || aboutData.Count == 0)
                return View("Empty");

            return View(aboutData[0]);
        }
    }
}



