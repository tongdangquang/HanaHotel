using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactController : ControllerBase
    {
        private readonly IContactService _contactService;
        public ContactController(IContactService contactService)
        {
            _contactService = contactService;
        }

        [HttpGet]
        public IActionResult GetContacts()
        {
            var contact = _contactService.TGetList();
            return Ok(contact);
        }

        [HttpGet("{id}")]
        public IActionResult GetContact(int id)
        {
            var contact = _contactService.TGetByID(id);
            return Ok(contact);
        }

        [HttpPut]
        public IActionResult UpdateContact(Contact contact)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _contactService.TUpdate(contact);
            return Ok("Updated Successfully");
        }

        [HttpPost]
        public IActionResult AddContact(Contact contact)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            contact.Date = Convert.ToDateTime(DateTime.Now.ToString());
            _contactService.TInsert(contact);

            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteContact(int id)
        {
            var contact = _contactService.TGetByID(id);
            if (contact == null)
            {
                return NotFound();
            }

            _contactService.TDelete(contact);
            return NoContent();
        }

        [HttpGet("replied-count")]
        public IActionResult GetRepliedContactCount()
        {
            return Ok(_contactService.TGetRepliedContactsCount());
        }

        [HttpGet("unreplied-count")]
        public IActionResult GetUnrepliedContactCount()
        {
            return Ok(_contactService.TGetUnRepliedContactCount());
        }

        [HttpGet("category/{categoryId}")]
        public IActionResult GetSpecificCategoryContacts(int categoryId)
        {
            var contacts = _contactService.TGetSpesificCategoryContacts(categoryId);

            if (contacts == null || contacts.Count == 0)
                return NotFound("No contacts found for the specified category.");

            return Ok(contacts);
        }

        [HttpGet("get-contact-with-category")]
        public IActionResult GetContactWithCategory()
        {
            return Ok(_contactService.TGetContactWithCategory());
        }


    }
}
