using BidenSurfer.Infras.Models;
using BidenSurfer.WebApi.Helpers;
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

        [HttpPost("login")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);

            if (response == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(response);
        }

        [HttpGet("all")]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }
        
        [HttpGet("get-balance")]
        [Authorize]
        public async Task<IActionResult> GetBalance()
        {
            var balanceDto = await _userService.GetBalance();
            return Ok(balanceDto);
        }

        [HttpGet("byId")]
        public async Task<IActionResult> GetById([FromQuery]long id)
        {
            var user = await _userService.GetById(id);
            return Ok(user);
        }

        [HttpGet("gen-hash")]
        public IActionResult GenHash([FromQuery] string text)
        {
            var hash = _userService.GenHash(text);
            return Ok(hash);
        }

        [HttpPost("addoredit")]
        public async Task<IActionResult> AddOrEdit(UserDto user)
        {
            var isSuccess = await _userService.AddOrEdit(user);
            return Ok(isSuccess);
        }

        [HttpGet("api-setting")]
        [Authorize]
        public async Task<IActionResult> ApiSetting()
        {
            var setting = await _userService.GetApiSetting();
            return Ok(setting);
        }


        [HttpPost("save-api-setting")]
        [Authorize]
        public async Task<IActionResult> SaveApiSetting(UserSettingDto setting)
        {
            await _userService.SaveApiSetting(setting);
            return Ok();
        }

        [HttpGet("general-setting")]
        [Authorize]
        public async Task<IActionResult> GeneralSetting()
        {
            var setting = await _userService.GetGeneralSetting();
            return Ok(setting);
        }


        [HttpPost("save-general-setting")]
        [Authorize]
        public async Task<IActionResult> SaveGeneralSetting(GeneralSettingDto setting)
        {
            var rs = await _userService.SaveGeneralSetting(setting);
            return Ok(rs);
        }

        [HttpGet("max-borrow")]
        [Authorize]
        public async Task<IActionResult> GetMaxBorrow([FromQuery] string symbol, [FromQuery] string orderSide)
        {
            var maxAmount = await _userService.GetMaximumBorrow(symbol, orderSide);
            return Ok(maxAmount);
        }

        [Authorize]
        [HttpPost("start-stop-bot")]
        public async Task<IActionResult> StartStopBot(GeneralSettingDto setting)
        {
            var rs = await _userService.StartStopBot(setting);
            return Ok(rs);
        }

        [Authorize]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetBot(GeneralSettingDto setting)
        {
            var rs = await _userService.StartStopBot(setting);
            return Ok(rs);
        }
    }
}