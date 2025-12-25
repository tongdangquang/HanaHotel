using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RapidApiConsume.Models;

public class ImdbController : Controller
{
    public async Task<IActionResult> Index()
    {
        List<ApiMovieViewModel> movieDataList = new List<ApiMovieViewModel>();

        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("https://advance-movie-api.p.rapidapi.com/api/v1/streamitfree/upcoming"),
            Headers =
            {
                { "x-rapidapi-key", "de2a56ed95mshfb93773087ad564p1f3f70jsn5e057d814bfc" },
                { "x-rapidapi-host", "advance-movie-api.p.rapidapi.com" },
            },
        };

        using (var response = await client.SendAsync(request))
        {
            response.EnsureSuccessStatusCode();
            var body = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(body)!;
            movieDataList = apiResponse.Result.Data;
        }
        return View(movieDataList);
    }
}
