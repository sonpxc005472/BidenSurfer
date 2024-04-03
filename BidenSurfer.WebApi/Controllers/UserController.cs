using BidenSurfer.Infras.Models;
using BidenSurfer.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidenSurfer.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetById([FromQuery]long id)
        {
            var user = await _userService.GetById(id);
            return Ok(user);
        }
        
        [HttpPost("addoredit")]
        public async Task<IActionResult> AddOrEdit(UserDto user)
        {
            var isSuccess = await _userService.AddOrEdit(user);
            return Ok(isSuccess);
        }
    }
}