using BidenSurfer.Infras.Models;
using BidenSurfer.WebApi.Helpers;
using BidenSurfer.WebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BidenSurfer.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ScannerController : ControllerBase
    {
        private IScannerService _scannerService;
        private ISecurityContextAccessor _securityContextAccessor;
        public ScannerController(IScannerService scannerService, ISecurityContextAccessor securityContextAccessor)
        {
            _scannerService = scannerService;
            _securityContextAccessor = securityContextAccessor;
        }
                
        [Authorize]
        [HttpGet("getall")]
        public async Task<IActionResult> GetAll()
        {
            var userId = _securityContextAccessor.UserId;
            var scanners = await _scannerService.GetScannerConfigsByUser(userId);
            return Ok(scanners);
        }
                
        [Authorize]
        [HttpGet("getbyid")]
        public async Task<IActionResult> GetById([FromQuery] long id)
        {
            var scanner = await _scannerService.GetById(id);
            return Ok(scanner);
        }
        
        [Authorize]
        [HttpGet("setting")]
        public async Task<IActionResult> GetScannerSetting()
        {
            var userId = _securityContextAccessor.UserId;
            var scannersetting = await _scannerService.GetScannerSettingByUser(userId);
            return Ok(scannersetting);
        }
        
        [Authorize]
        [HttpPost("upsert")]
        public async Task<IActionResult> AddOrEdit(ScannerDto config)
        {
            var rs = await _scannerService.AddOrEdit(config);
            return Ok(rs);
        }
        
        [Authorize]
        [HttpPost("upsert-setting")]
        public async Task<IActionResult> AddOrEditSetting(ScannerSettingDto setting)
        {
            var userId = _securityContextAccessor.UserId;
            setting.UserId = userId;
            var rs = await _scannerService.AddOrEditScannerSetting(setting);
            return Ok(rs);
        }

        [Authorize]
        [HttpPost("delete")]
        public async Task<IActionResult> Delete(long configId)
        {
            var rs = await _scannerService.Delete(configId);
            return Ok(rs);
        }
    }
}