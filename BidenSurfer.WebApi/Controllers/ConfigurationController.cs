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
        public async Task<IActionResult> AddOrEdit(AddEditConfigDto config)
        {
            var issuccess = await _configService.AddOrEdit(config, false);
            return Ok(issuccess);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete([FromQuery]long configId)
        {
            var rs = await _configService.Delete(configId);
            return Ok(rs);
        }

        [Authorize]
        [HttpGet("symbols")]
        public async Task<IActionResult> GetSymbols()
        {
            var rs = await _configService.GetAllMarginSymbol();
            return Ok(rs);
        }

        [Authorize]
        [HttpPut("set-active")]
        public async Task<IActionResult> SetActive(SetActiveDto data)
        {
            var rs = await _configService.SetConfigActiveStatus(data.Id, data.IsActive);
            return Ok(rs);
        }
    }
}