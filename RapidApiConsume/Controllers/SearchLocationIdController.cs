using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RapidApiConsume.Models;

namespace RapidApiConsume.Controllers
{
    public class SearchLocationIdController : Controller
    {
        public async Task<IActionResult> Index([FromForm] string cityName)
        {
            cityName = cityName ?? "paris";
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com15.p.rapidapi.com/api/v1/Hotels/searchDestination?query={cityName}"),
                Headers =
            {
                { "x-rapidapi-key", "829c4ea854msh9bb0e6eda455a44p153742jsn4eb4c970dc8d" },
                { "x-rapidapi-host", "booking-com15.p.rapidapi.com" },
            },
            };
            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                LocationViewModel.Rootobject apiResponse = JsonConvert.DeserializeObject<LocationViewModel.Rootobject>(body)!;
                LocationViewModel.Datum city = apiResponse!.data[0];
                return View(city);
            }
        }
    }
}

