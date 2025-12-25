using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RapidApiConsume.Models;

namespace RapidApiConsume.Controllers
{
    public class HanaHotelController : Controller
    {
        public async Task<IActionResult> Index([FromQuery] string dest_id)
        {
            dest_id = dest_id ?? "-1456928";

            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://booking-com15.p.rapidapi.com/api/v1/Hotels/searchHotels?dest_id={dest_id}&search_type=CITY&arrival_date=2024-09-25&departure_date=2024-09-29&adults=1&children_age=0%2C17&room_qty=1&page_number=1&units=metric&temperature_unit=c&languagecode=en-us&currency_code=EUR"),
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
                HotelViewModel.Rootobject rootObject = JsonConvert.DeserializeObject<HotelViewModel.Rootobject>(body)!;
                HotelViewModel.Data data = rootObject.data;
                List<HotelViewModel.Hotel> Hotels = data.Hotels.ToList();
                return View(Hotels);
            }
        }
    }
}
