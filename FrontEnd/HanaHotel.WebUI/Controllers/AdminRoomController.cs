using HanaHotel.WebUI.DTOs.RoomDTO;
using HanaHotel.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace HanaHotel.WebUI.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminRoomController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiUrl;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminRoomController> _logger;

        public AdminRoomController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings, IWebHostEnvironment env, ILogger<AdminRoomController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
            _env = env;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Room");
            if (responseMessage.IsSuccessStatusCode)
            {
                var jsonData = await responseMessage.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<List<ResultRoomDTO>>(jsonData);
                return View(values);
            }
            return View(new List<ResultRoomDTO>());
        }

        [HttpGet]
        public IActionResult AddRoom()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddRoom(AddRoomDTO model, List<IFormFile>? Images)
        {
            try
            {
                _logger.LogInformation("AdminRoomController.AddRoom called. Files bound: {Count}", Images?.Count ?? 0);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState invalid when adding room");
                    return View(model);
                }

                model.ImagePaths = model.ImagePaths ?? new List<string>();

                if (Images != null && Images.Any())
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var targetDir = Path.Combine(webRoot, "hotel-html-template", "img");
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    foreach (var file in Images)
                    {
                        if (file == null || file.Length == 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        var fileName = $"{Guid.NewGuid():N}{ext}";
                        var physicalPath = Path.Combine(targetDir, fileName);

                        using (var stream = new FileStream(physicalPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var savedPath = $"hotel-html-template/img/{fileName}";
                        model.ImagePaths.Add(savedPath);
                        _logger.LogInformation("Saved image to {Path}", savedPath);
                    }
                }
                else
                {
                    _logger.LogInformation("No images uploaded in AddRoom request.");
                }

                // Log payload to send to API
                var client = _httpClientFactory.CreateClient();
                var jsonData = JsonConvert.SerializeObject(model);
                _logger.LogDebug("POST /api/Room payload: {Payload}", jsonData);

                var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiUrl}/api/Room", jsonContent);

                var respText = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API AddRoom succeeded");
                    return RedirectToAction("Index");
                }
                else
                {
                    _logger.LogError("API AddRoom failed. Status: {Status}. Response: {Response}", response.StatusCode, respText);
                    ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi khi thêm phòng. API trả về: {respText}");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in AddRoom");
                ModelState.AddModelError(string.Empty, "Lỗi khi xử lý upload ảnh. Kiểm tra logs.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpdateRoom(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_apiUrl}/api/Room/{id}");

            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var values = JsonConvert.DeserializeObject<UpdateRoomDTO>(jsonData);

                if (values != null)
                    return View(values);

                ModelState.AddModelError(string.Empty, "Không tìm thấy dữ liệu phòng.");
            }
            else
                ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi lấy dữ liệu phòng.");

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRoom(UpdateRoomDTO model, List<IFormFile>? Images, [FromForm(Name = "removeImageIds")] int[]? removeImageIds, [FromForm(Name = "removeImagePaths")] string[]? removeImagePaths)
        {
            try
            {
                _logger.LogInformation("AdminRoomController.UpdateRoom called. Files bound: {Count}, RemoveIds: {RemoveIdsCount}, RemovePaths: {RemovePathsCount}", Images?.Count ?? 0, removeImageIds?.Length ?? 0, removeImagePaths?.Length ?? 0);

                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState invalid when updating room");
                    return View(model);
                }

                // bind remove arrays into model
                if (removeImageIds != null && removeImageIds.Length > 0)
                    model.RemoveImageIds = removeImageIds;
                if (removeImagePaths != null && removeImagePaths.Length > 0)
                    model.RemoveImagePaths = removeImagePaths;

                model.ImagePaths = model.ImagePaths ?? new List<string>();

                if (Images != null && Images.Any())
                {
                    var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    var targetDir = Path.Combine(webRoot, "hotel-html-template", "img");
                    if (!Directory.Exists(targetDir))
                        Directory.CreateDirectory(targetDir);

                    foreach (var file in Images)
                    {
                        if (file == null || file.Length == 0) continue;
                        var ext = Path.GetExtension(file.FileName);
                        var fileName = $"{Guid.NewGuid():N}{ext}";
                        var physicalPath = Path.Combine(targetDir, fileName);

                        using (var stream = new FileStream(physicalPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        var savedPath = $"hotel-html-template/img/{fileName}";
                        model.ImagePaths.Add(savedPath);
                        _logger.LogInformation("Saved image to {Path}", savedPath);
                    }
                }
                else
                {
                    _logger.LogInformation("No new images uploaded in UpdateRoom request.");
                }

                var client = _httpClientFactory.CreateClient();
                var jsonData = JsonConvert.SerializeObject(model);
                _logger.LogDebug("PUT /api/Room payload: {Payload}", jsonData);

                var stringContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PutAsync($"{_apiUrl}/api/Room", stringContent);

                var respText = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("API UpdateRoom succeeded");
                    // Optionally delete files on frontend wwwroot if model.RemoveImagePaths provided (already implemented previously)
                    return RedirectToAction("Index");
                }

                _logger.LogError("API UpdateRoom failed. Status: {Status}. Response: {Response}", response.StatusCode, respText);
                ModelState.AddModelError(string.Empty, $"Đã xảy ra lỗi khi cập nhật phòng. API trả về: {respText}");
                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in UpdateRoom");
                ModelState.AddModelError(string.Empty, "Lỗi khi xử lý upload ảnh. Kiểm tra logs.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/Room/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa phòng.");
            return RedirectToAction("Index");
        }
    }
}
