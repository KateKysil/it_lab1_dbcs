using Microsoft.AspNetCore.Mvc;
using it_lab1_dbcs.Storage;

namespace it_lab1_dbcs.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager _userManager;

        public AuthController(UserManager userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] AuthRequest dto)
        {
            var (success, message) = _userManager.Register(dto.Username, dto.Password);
            if (!success)
            {
                return BadRequest(new { message });
            }
            return Ok(new { message });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] AuthRequest dto)
        {
            var (success, message) = _userManager.Login(dto.Username, dto.Password);
            if (!success)
            {
                return Unauthorized(new { message });
            }
            return Ok(new { username = dto.Username, message });
        }
    }

    public class AuthRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}