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
        private ISecurityContextAccessor _securityContextAccessor;

        public ConfigurationController(IConfigService configService, ISecurityContextAccessor securityContextAccessor)
        {
            _configService = configService;
            _securityContextAccessor = securityContextAccessor;
        }
                
        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var userId = _securityContextAccessor.UserId;
            var configs = await _configService.GetConfigsByUser(userId);
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
            var userId = _securityContextAccessor.UserId;
            config.UserId = userId;
            var issuccess = await _configService.AddOrEdit(config);
            return Ok(issuccess);
        }

        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(long configId)
        {
            var rs = await _configService.Delete(configId);
            return Ok(rs);
        }
    }
}