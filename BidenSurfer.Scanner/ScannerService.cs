
using BidenSurfer.Infras;
using BidenSurfer.Infras.Models;

namespace BidenSurfer.BotRunner.Services;
public interface IScannerService
{
    List<ScannerDto> GetAll();
}

public class ScannerService : IScannerService
{
    private readonly IRedisCacheService _redisCacheService;

    public ScannerService(IRedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    public List<ScannerDto> GetAll()
    {
        List<ScannerDto> resultDto = new List<ScannerDto>();
        var cachedData = _redisCacheService.GetCachedData<List<ScannerDto>>(AppConstants.RedisAllScanners);
        if (cachedData != null)
        {
            return cachedData.Where(c=>c.IsActive).ToList();
        }        
        return resultDto;
    }
}