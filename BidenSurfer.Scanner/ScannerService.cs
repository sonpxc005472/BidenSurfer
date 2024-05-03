
using BidenSurfer.Infras;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

namespace BidenSurfer.BotRunner.Services;
public interface IScannerService
{
    Task<List<ScannerDto>> GetAll();
    Task<List<ScannerSettingDto>> GetScannerSettings();

}

public class ScannerService : IScannerService
{
    private readonly AppDbContext _context;
    public ScannerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ScannerDto>> GetAll()
    {
        List<ScannerDto> resultDto = new List<ScannerDto>();
        try
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
            return resultDto;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return resultDto;
        }
        
    }

    public async Task<List<ScannerSettingDto>> GetScannerSettings()
    {
        try
        {
            var result = await _context.ScannerSetting.ToListAsync();
            if(result != null)
            {
                var resultDto = result.ConvertAll(c => new ScannerSettingDto
                {
                    Id = c.Id,
                    UserId = c.Userid,
                    BlackList = c.BlackList,
                    MaxOpen = c.MaxOpen
                });
                StaticObject.AllScannerSetting = resultDto;                
            }
            
            return StaticObject.AllScannerSetting;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return StaticObject.AllScannerSetting;
        }
    }
}