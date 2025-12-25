using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using HanaHotel.BusinessLayer.Abstract;
using HanaHotel.EntityLayer.Concrete;

namespace HanaHotel.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AboutController : ControllerBase
    {
        private readonly IAboutService _aboutService;
        private readonly IMapper _mapper;


        public AboutController(IAboutService aboutService, IMapper mapper)
        {
            _aboutService = aboutService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAbouts()
        {
            var Abouts = _aboutService.TGetList();
            return Ok(Abouts);
        }

        [HttpGet("{id}")]
        public IActionResult GetAbout(int id)
        {
            var About = _aboutService.TGetByID(id);
            return Ok(About);
        }

        [HttpPut]
        public IActionResult UpdateAbout(About about)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            _aboutService.TUpdate(about);
            return Ok("Updated Successfully");
        }

        [HttpPost]
        public IActionResult AddAbout(About about)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _aboutService.TInsert(about);

            return Ok();
        }

        [HttpDelete]
        public IActionResult DeleteAbout(int id)
        {
            var About = _aboutService.TGetByID(id);
            if (About == null)
            {
                return NotFound();
            }

            _aboutService.TDelete(About);
            return NoContent();
        }
    }
}
