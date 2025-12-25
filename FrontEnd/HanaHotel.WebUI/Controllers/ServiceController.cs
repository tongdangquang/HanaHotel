using HanaHotel.WebUI.DTOs.ServiceDTO;
using HanaHotel.WebUI.DTOs.RoomDTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace HanaHotel.WebUI.Controllers
{
    public class ServiceController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;

        public ServiceController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = configuration["AppSettings:urlAPI"]
                      ?? throw new Exception("Không tìm thấy cấu hình AppSettings:urlAPI trong appsettings.json");
        }

        // ============================ INDEX ============================
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Service");

            if (!response.IsSuccessStatusCode)
                return View(new List<ResultServiceDTO>());

            var json = await response.Content.ReadAsStringAsync();
            var services = JsonConvert.DeserializeObject<List<ResultServiceDTO>>(json);

            return View(services);
        }

        // ============================ CREATE (GET) ============================
        [HttpGet]
        public async Task<IActionResult> AddService()
        {
            await LoadRooms(null);
            return View();
        }

        // ============================ CREATE (POST) ============================
        [HttpPost]
        public async Task<IActionResult> AddService(CreateServiceDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRooms(null);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PostAsync($"{_apiUrl}/api/Service", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Đã xảy ra lỗi khi thêm dịch vụ.");
            await LoadRooms(null);
            return View(model);
        }

        // ============================ EDIT (GET) ============================
        [HttpGet]
        public async Task<IActionResult> UpdateService(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Service/{id}");

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "Không tìm thấy dịch vụ.");
                await LoadRooms(null);
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ResultServiceDTO>(json);

            if (result == null)
            {
                ModelState.AddModelError("", "Không tìm thấy dữ liệu dịch vụ.");
                await LoadRooms(null);
                return View();
            }

            var dto = new UpdateServiceDTO
            {
                Id = result.Id,
                ServiceName = result.ServiceName,
                Price = result.Price,
                Unit = result.Unit,
                Description = result.Description,
                RoomIds = result.Rooms?.Select(r => r.Id).ToList() ?? new List<int>()
            };

            await LoadRooms(dto.RoomIds);
            return View(dto);
        }

        // ============================ EDIT (POST) ============================
        [HttpPost]
        public async Task<IActionResult> UpdateService(UpdateServiceDTO model)
        {
            if (!ModelState.IsValid)
            {
                await LoadRooms(model.RoomIds);
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var content = new StringContent(
                JsonConvert.SerializeObject(model),
                Encoding.UTF8,
                "application/json"
            );

            var response = await client.PutAsync($"{_apiUrl}/api/Service/{model.Id}", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Đã xảy ra lỗi khi cập nhật dịch vụ.");
            await LoadRooms(model.RoomIds);
            return View(model);
        }

        // ============================ DELETE ============================
        public async Task<IActionResult> DeleteService(int id)
        {
            var client = _httpClientFactory.CreateClient();
            await client.DeleteAsync($"{_apiUrl}/api/Service/{id}");
            return RedirectToAction("Index");
        }

        // ============================ LOAD ROOMS ============================
        private async Task LoadRooms(List<int>? selectedRoomIds)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Room");

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Rooms = new List<ResultRoomDTO>();
                ViewBag.SelectedRoomIds = new List<int>();
                return;
            }

            var json = await response.Content.ReadAsStringAsync();

            ViewBag.Rooms = JsonConvert.DeserializeObject<List<ResultRoomDTO>>(json)
                            ?? new List<ResultRoomDTO>();

            ViewBag.SelectedRoomIds = selectedRoomIds ?? new List<int>();
        }
    }
}
