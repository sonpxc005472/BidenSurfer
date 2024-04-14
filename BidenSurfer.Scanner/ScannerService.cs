
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

namespace BidenSurfer.BotRunner.Services;
public interface IScannerService
{
    Task<List<ScannerDto>> GetAll();
    void DeleteAll();
    Task<List<ScannerSettingDto>> GetAllScannerSetting();

}

public class ScannerService : IScannerService
{
    private readonly IRedisCacheService _redisCacheService;
    private readonly AppDbContext _context;
    public ScannerService(IRedisCacheService redisCacheService, AppDbContext context)
    {
        _redisCacheService = redisCacheService;
        _context = context;
    }

    public void DeleteAll()
    {
        _redisCacheService.RemoveCachedData(AppConstants.RedisAllScanners);
    }

    public async Task<List<ScannerDto>> GetAll()
    {
        List<ScannerDto> resultDto = new List<ScannerDto>();
        try
        {
            var cachedData = _redisCacheService.GetCachedData<List<ScannerDto>>(AppConstants.RedisAllScanners);
            if (cachedData != null)
            {
                var activeData = cachedData.Where(c => c.IsActive).ToList();
                StaticObject.AllScanners = activeData;
                return activeData;
            }
            else
            {
                var result = await _context.Scanners.ToListAsync();
                resultDto = result.Select(r => new ScannerDto
                {
                    Id = r.Id,
                    UserId = r.Userid,
                    PositionSide = r.PositionSide,
                    Title = r.Title,
                    OrderChange = r.OrderChange,
                    IsActive = r.IsActive,
                    Amount = r.Amount,
                    OrderType = r.OrderType,
                    AmountLimit = r.AmountLimit,
                    AmountExpire = r.AmountExpire,
                    BlackList = r.BlackList,
                    OnlyPairs = r.OnlyPairs,
                    ConfigExpire = r.ConfigExpire,
                    Elastic = r.Elastic,
                    AutoAmount = r.AutoAmount,
                    OcNumber = r.OcNumber,
                    Turnover = r.Turnover
                }).ToList();
                StaticObject.AllScanners = resultDto;
                _redisCacheService.SetCachedData(AppConstants.RedisAllScanners, resultDto, TimeSpan.FromDays(10));
            }
            return resultDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return resultDto;
        }
        
    }

    public async Task<List<ScannerSettingDto>> GetAllScannerSetting()
    {
        List<ScannerSettingDto> resultDto = new List<ScannerSettingDto>();
        try
        {
            var cachedData = _redisCacheService.GetCachedData<List<ScannerSettingDto>>(AppConstants.RedisAllScannerSetting);
            if (cachedData != null)
            {
                StaticObject.AllScannerSetting = cachedData;
                return cachedData;
            }
            else
            {
                var result = await _context.ScannerSetting.ToListAsync();
                resultDto = result.Select(r => new ScannerSettingDto
                {
                    Id = r.Id,
                    UserId = r.Userid,
                    BlackList = r.BlackList,
                    MaxOpen = r.MaxOpen
                }).ToList();
                StaticObject.AllScannerSetting = resultDto;
                _redisCacheService.SetCachedData(AppConstants.RedisAllScannerSetting, resultDto, TimeSpan.FromDays(10));
            }
            return resultDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return resultDto;
        }
    }
}