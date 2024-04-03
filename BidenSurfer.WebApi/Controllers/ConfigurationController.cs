using BidenSurfer.Infras.Models;
using BidenSurfer.WebApi.Helpers;
using BidenSurfer.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidenSurfer.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConfigurationController : ControllerBase
    {
        private IConfigService _configService;

        public ConfigurationController(IConfigService configService)
        {
            _configService = configService;
        }
                
        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var configs = await _configService.GetByActiveUser();
            return Ok(configs);
        }
        
        [Authorize]
        [HttpGet("getbyuser")]
        public async Task<IActionResult> GetByUser([FromQuery] long userid)
        {
            var configs = await _configService.GetConfigsByUser(userid);
            return Ok(configs);
        }

        [Authorize]
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetById([FromQuery] long id)
        {
            var config = await _configService.GetById(id);
            return Ok(config);
        }
        
        [Authorize]
        [HttpPost("addoredit")]
        public async Task<IActionResult> AddOrEdit(ConfigDto config)
        {
            var issuccess = await _configService.AddOrEdit(config);
            return Ok(issuccess);
        }
    }
}