using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageCategoryController : ControllerBase
    {
        private readonly IMessageCategoryService _messageCategoryService;

        public MessageCategoryController(IMessageCategoryService messageCategoryService)
        {
            _messageCategoryService = messageCategoryService;
        }

        [HttpGet]
        public IActionResult GetMessageCategories()
        {
            var messageCategories = _messageCategoryService.TGetList();
            return Ok(messageCategories);
        }

        [HttpGet("{id}")]
        public IActionResult GetMessageCategory(int id)
        {
            var messageCategory = _messageCategoryService.TGetByID(id);
            if (messageCategory == null)
                return NotFound();

            return Ok(messageCategory);
        }

        [HttpPut]
        public IActionResult UpdateMessageCategory(MessageCategory messageCategory)
        {
            if (!ModelState.IsValid || messageCategory == null)
                return BadRequest(ModelState);

            _messageCategoryService.TUpdate(messageCategory);
            return Ok("Updated Successfully");
        }

        [HttpPost]
        public IActionResult AddMessageCategory(MessageCategory messageCategory)
        {
            if (!ModelState.IsValid || messageCategory == null)
                return BadRequest(ModelState);

            _messageCategoryService.TInsert(messageCategory);
            return CreatedAtAction(nameof(GetMessageCategory), new { id = messageCategory.MessageCategoryId }, messageCategory);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteMessageCategory(int id)
        {
            var messageCategory = _messageCategoryService.TGetByID(id);
            if (messageCategory == null)
                return NotFound();

            _messageCategoryService.TDelete(messageCategory);
            return NoContent();
        }
    }
}
