using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiJwt.Models;

namespace WebApiJwt.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet("generate-token")]
        public IActionResult GenerateToken()
        {
            return Ok(new CreateToken().Create());
        }

        [HttpGet("generate-admin-token")]
        public IActionResult GenerateAdminToken()
        {
            return Ok(new CreateToken().CreateAdmin());
        }

        [Authorize]
        [HttpGet("authenticated")]
        public IActionResult Authenticated()
        {
            return Ok("Welcome");
        }

        [Authorize(Roles = "Admin,Guest")]
        [HttpGet("admin-access")]
        public IActionResult AdminAccess()
        {
            return Ok("Welcome");
        }
    }
}
