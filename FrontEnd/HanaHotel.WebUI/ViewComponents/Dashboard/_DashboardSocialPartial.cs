using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using HanaHotel.WebUI.DTOs.DashboardDTO.DashboardSocialPartialDTO;

namespace HanaHotel.WebUI.ViewComponents.Dashboard
{
    public class _DashboardSocialPartial : ViewComponent
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public _DashboardSocialPartial(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var data = new Dictionary<string, string>();

            data.Add("Instagram", await FetchSocialCountAsync(FetchInstagramCount));
            data.Add("Linkedin", await FetchSocialCountAsync(FetchLinkedinCount));
            data.Add("Github", await FetchSocialCountAsync(FetchGithubCount));

            return View(data);
        }

        private async Task<string> FetchSocialCountAsync(Func<Task<string>> fetchFunction)
        {
            try
            {
                return await fetchFunction();
            }
            catch (HttpRequestException ex)
            {
                return "0";
            }
        }

        private async Task<string> FetchInstagramCount()
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://instagram243.p.rapidapi.com/userfollowers/3072447324/12/%7Bend_cursor%7D"),
                Headers =
                {
                    { "x-rapidapi-key", "de2a56ed95mshfb93773087ad564p1f3f70jsn5e057d814bfc" },
                    { "x-rapidapi-host", "instagram243.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<InstagramCountModel.Rootobject>(body);
                    return apiResponse?.data.edge_followed_by.count.ToString() ?? "0";
                }
                return "0";
            }
        }

        private async Task<string> FetchLinkedinCount()
        {
            var client = _httpClientFactory.CreateClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://linkedin-data-api.p.rapidapi.com/connection-count?username=hasan-uslu"),
                Headers =
                {
                    { "x-rapidapi-key", "de2a56ed95mshfb93773087ad564p1f3f70jsn5e057d814bfc" },
                    { "x-rapidapi-host", "linkedin-data-api.p.rapidapi.com" },
                },
            };

            using (var response = await client.SendAsync(request))
            {
                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<LinkedinCountModel.Rootobject>(body);
                    return apiResponse?.connection.ToString() ?? "0";
                }
                return "0";
            }
        }

        private async Task<string> FetchGithubCount()
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("HanaHotelAPI");

            var responseMessage = await client.GetAsync("https://api.github.com/users/hzuslu/followers");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<dynamic>>(jsonData);
                return values?.Count.ToString() ?? "0";
            }
            return "0";
        }
    }
}
