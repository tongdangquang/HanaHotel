using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ContactDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.ViewComponents.Dashboard
{
    public class _DashboardContactPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMapper _mapper;
        private readonly string _apiUrl;

        public _DashboardContactPartial(IHttpClientFactory httpClientFactory, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _mapper = mapper;
            _apiUrl = appSettings.Value.urlAPI;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Contact");

            if (!responseMessage.IsSuccessStatusCode)
                return View();

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<AdminDashboardContactDTO>>(jsonData);


            return View(values!.Take(6).ToList());
        }
    }
}
