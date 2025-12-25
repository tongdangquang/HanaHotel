using HanaHotel.EntityLayer.Concrete;
using HanaHotel.WebUI.DTOs.ContactDTO;
using HanaHotel.WebUI.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using Newtonsoft.Json;

namespace HanaHotel.WebUI.Controllers
{

	[Authorize(Roles = "Admin")]
	public class AdminContactController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
		private readonly string _apiUrl;

		public AdminContactController(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _apiUrl = appSettings.Value.urlAPI;
		}

        [HttpGet]
        public async Task<IActionResult> Inbox([FromQuery] int? categoryId)
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync(categoryId != null
                                   ? $"{_apiUrl}/api/Contact/category/{categoryId}"
                                   : $"{_apiUrl}/api/Contact");


            if (!responseMessage.IsSuccessStatusCode)
                return View();

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<InboxContactDTO>>(jsonData);
            ViewBag.InboxCount = values!.Count;
            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> ReadEmail([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID email không hợp lệ.");

            var client = _httpClientFactory.CreateClient();


            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Contact/{id}");

            if (!responseMessage.IsSuccessStatusCode)
                return NotFound("Không tìm thấy email.");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<DetailContactDTO>(jsonData);

            return View(value);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteEmail(int id)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.DeleteAsync($"{_apiUrl}/api/Contact/{id}");
            if (response.IsSuccessStatusCode)
                return RedirectToAction("Inbox");

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi xóa email.");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> OutgoingMessages()
        {
            var client = _httpClientFactory.CreateClient();
            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Contact/replied-count");

            if (!responseMessage.IsSuccessStatusCode)
                return View();

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var values = JsonConvert.DeserializeObject<List<OutgoingMessagesDTO>>(jsonData);

            return View(values);
        }

        [HttpGet]
        public async Task<IActionResult> SendEmail([FromRoute] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("ID email không hợp lệ.");

            var client = _httpClientFactory.CreateClient();

            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Contact/{id}");

            if (!responseMessage.IsSuccessStatusCode)
                return NotFound("Không tìm thấy email.");

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var value = JsonConvert.DeserializeObject<SendMessageDTO>(jsonData);

            return View(value);
        }

        [HttpPost]
        public async Task<IActionResult> SendEmail(SendMessageDTO sendMessageDTO)
        {
            if (sendMessageDTO == null || sendMessageDTO.ContactID <= 0)
            {
                ModelState.AddModelError(string.Empty, "Dữ liệu phản hồi không hợp lệ.");
                return View(sendMessageDTO);
            }

            var client = _httpClientFactory.CreateClient();

            var responseMessage = await client.GetAsync($"{_apiUrl}/api/Contact/{sendMessageDTO.ContactID}");

            if (!responseMessage.IsSuccessStatusCode)
            {
                return NotFound("Không tìm thấy email");
            }

            var jsonData = await responseMessage.Content.ReadAsStringAsync();
            var contactEntity = JsonConvert.DeserializeObject<Contact>(jsonData);

            if (contactEntity == null)
            {
                ModelState.AddModelError(string.Empty, "Không thể lấy thông tin liên hệ.");
                return View(sendMessageDTO);
            }

            contactEntity.ResponseMessage = sendMessageDTO.ResponseMessage;
            contactEntity.IsReplied = true;
            contactEntity.ResponseDate = DateTime.Now;

            var updatedJsonData = JsonConvert.SerializeObject(contactEntity);
            var stringContent = new StringContent(updatedJsonData, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"{_apiUrl}/api/Contact", stringContent);

            if (response.IsSuccessStatusCode)
            {
                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress("Hasan Uslu - CEO OF HanaHotelIER", "dumenmail123@gmail.com"));
                mimeMessage.To.Add(new MailboxAddress("Khách hàng", contactEntity.Email));

                var bodyBuilder = new BodyBuilder
                {
                    TextBody = sendMessageDTO.ResponseMessage
                };
                mimeMessage.Body = bodyBuilder.ToMessageBody();
                mimeMessage.Subject = "Phản hồi về email bạn đã gửi";

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.ConnectAsync("smtp.gmail.com", 587, false);
                    await smtpClient.AuthenticateAsync("dumenmail123@gmail.com", "klryoieeoreqdqro");
                    await smtpClient.SendAsync(mimeMessage);
                    await smtpClient.DisconnectAsync(true);
                }

                return RedirectToAction("Inbox");
            }

            ModelState.AddModelError(string.Empty, "Đã xảy ra lỗi khi gửi phản hồi.");
            return View(sendMessageDTO);
        }

    }
}
