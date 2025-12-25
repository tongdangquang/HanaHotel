using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ServiceDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HotelProject.WebUI.ViewComponents.Default
{
    public class _ServicePartial : ViewComponent
    {

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;    

        public _ServicePartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Service");

            if (!responseMessage.IsSuccessStatusCode)
                return View("Error");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var aboutData = JsonConvert.DeserializeObject<List<ResultServiceDTO>>(jsonData);

            if (aboutData == null || aboutData.Count == 0)
                return View("Empty");

            var roomsToDisplay = aboutData.Take(6).ToList(); // İlk 6 veriyi al


            return View(roomsToDisplay);
        }
    }
}
