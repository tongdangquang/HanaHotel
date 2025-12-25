using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ServiceDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HotelProject.WebUI.ViewComponents.Default
{
    public class _ServiceSelectPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _ServiceSelectPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        // selectedId optional to preselect
        public async Task<IViewComponentResult> InvokeAsync(int? selectedId)
        {
            var client = _httpClientFactory.CreateClient();
            List<ResultServiceDTO> services = new();

            try
            {
                var resp = await client.GetAsync($"{_apiUrl}/api/Service");
                if (resp.IsSuccessStatusCode)
                {
                    var json = await resp.Content.ReadAsStringAsync();
                    services = JsonConvert.DeserializeObject<List<ResultServiceDTO>>(json) ?? new List<ResultServiceDTO>();
                }
            }
            catch
            {
                // keep empty list on error; add logging if needed
            }

            ViewData["SelectedId"] = selectedId;
            return View(services);
        }
    }
}