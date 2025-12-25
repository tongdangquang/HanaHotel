using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.ContactDTO;
using HanaHotel.WebUI.DTOs.MessageCategoryDTO;
using Microsoft.Extensions.Options;
using HanaHotel.WebUI.Models;

namespace HanaHotel.WebUI.ViewComponents.Contact
{
    public class _ContactSidebarPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public _ContactSidebarPartial(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var client = _httpClientFactory.CreateClient();

            // Tüm iletişimleri al
            var contactValues = await GetContactsAsync(client);
            ViewBag.InboxCount = contactValues?.Count ?? 0;

            // Cevaplanan iletişimleri al
            var repliedContactValues = await GetRepliedContactsAsync(client);
            ViewBag.RepliedCount = repliedContactValues?.Count ?? 0;

            // Kategorileri al
            List<ResultMessageCategoryDTO> categories = await GetCategoriesAsync(client);
            ViewBag.Categories = categories;

            return View();
        }

        private async Task<List<InboxContactDTO>> GetContactsAsync(HttpClient client)
        {
            var response = await client.GetAsync($"{_apiUrl}/api/Contact");
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InboxContactDTO>>(jsonData)!;

        }

        private async Task<List<InboxContactDTO>> GetRepliedContactsAsync(HttpClient client)
        {
            var response = await client.GetAsync($"{_apiUrl}/api/Contact/replied-count");
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<InboxContactDTO>>(jsonData)!;
        }

        private async Task<List<ResultMessageCategoryDTO>> GetCategoriesAsync(HttpClient client)
        {
            var response = await client.GetAsync($"{_apiUrl}/api/MessageCategory");
            var jsonData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ResultMessageCategoryDTO>>(jsonData)!;
        }
    }
}
