using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.RoomDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HotelProject.WebUI.ViewComponents.Default
{
    public class _RoomSelectPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _RoomSelectPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        // selectedId is optional and used to preselect an option
        public async Task<IViewComponentResult> InvokeAsync(int? selectedId)
        {
            var client = _httpClientFactory.CreateClient();
            List<ResultRoomDTO> rooms = new();

            try
            {
                var response = await client.GetAsync($"{_apiUrl}/api/Room");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    rooms = JsonConvert.DeserializeObject<List<ResultRoomDTO>>(json) ?? new List<ResultRoomDTO>();
                }
            }
            catch
            {
                // swallow - view will render empty list. Consider logging if needed.
            }

            ViewData["SelectedId"] = selectedId;
            return View(rooms);
        }
    }
}