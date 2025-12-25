using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.ViewComponents.Dashboard
{
    public class _DashboardWidgetPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _DashboardWidgetPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/DashboardWidget");

            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);

                var colors = new List<string> { "gradient-1", "gradient-2", "gradient-3", "gradient-4" };
                var icons = new List<string> { "fa-group", "fa-users", "fa-home", "fa-check" };

                var model = new Tuple<Dictionary<string, string>, List<string>, List<string>>(values, colors, icons);
                return View(model);
            }
            else
            {
                var emptyModel = new Tuple<Dictionary<string, string>, List<string>, List<string>>(new Dictionary<string, string>(), new List<string>(), new List<string>());
                return View(emptyModel);
            }
        }


    }
}
