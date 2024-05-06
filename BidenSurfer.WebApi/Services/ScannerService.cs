namespace BidenSurfer.WebApi.Services;

using MassTransit;
using BidenSurfer.Infras;
using BidenSurfer.Infras.BusEvents;
using BidenSurfer.Infras.Domains;
using BidenSurfer.Infras.Entities;
using BidenSurfer.Infras.Models;
using Microsoft.EntityFrameworkCore;

public interface IScannerService
{
    Task<IEnumerable<ScannerDto>> GetScannerConfigsByUser(long userId);
    Task<IEnumerable<ScannerDto>> GetAll();
    Task<ScannerDto> GetById(long id);
    Task<bool> AddOrEdit(ScannerDto scanner);
    Task<bool> Delete(long id);
    Task<bool> SetScannerActiveStatus(long id, bool isActive);
    Task<ScannerSettingDto?> GetScannerSettingByUser(long userId);
    Task<IEnumerable<ScannerSettingDto>> GetAllScannerSetting();
    Task<bool> AddOrEditScannerSetting(ScannerSettingDto scannerSetting);
}

public class ScannerService : IScannerService
{
    private readonly AppDbContext _context;
    private readonly IBus _bus;

    public ScannerService(AppDbContext context, IBus bus)
    {
        _context = context;
        _bus = bus;
    }

    public async Task<bool> AddOrEdit(ScannerDto scanner)
    {
        var scannerEntity = await _context.Scanners?.FirstOrDefaultAsync(c => c.Id == scanner.Id);
        if (scannerEntity == null)
        {
            //Add new
            var scannerAdd = new Scanner
            {
                Userid = scanner.UserId,
                PositionSide = scanner.PositionSide,
                Title = scanner.Title,
                OrderChange = scanner.OrderChange,
                IsActive = scanner.IsActive,
                Amount = scanner.Amount,
                OrderType = scanner.OrderType,
                AmountLimit = scanner.AmountLimit,
                AmountExpire = scanner.AmountExpire,
                BlackList = scanner.BlackList,
                OnlyPairs = scanner.OnlyPairs,
                AutoAmount = scanner.AutoAmount,
                ConfigExpire = scanner.ConfigExpire,
                Elastic = scanner.Elastic,
                OcNumber = scanner.OcNumber,
                Turnover = scanner.Turnover
            };
            _context.Scanners.Add(scannerAdd);
            await _context.SaveChangesAsync();
        }
        else
        {
            //Edit
            scannerEntity.PositionSide = scanner.PositionSide;
            scannerEntity.Title = scanner.Title;
            scannerEntity.OrderChange = scanner.OrderChange;
            scannerEntity.IsActive = scanner.IsActive;
            scannerEntity.Amount = scanner.Amount;
            scannerEntity.OrderType = scanner.OrderType;
            scannerEntity.AmountLimit = scanner.AmountLimit;
            scannerEntity.AmountExpire = scanner.AmountExpire;
            scannerEntity.BlackList = scanner.BlackList;
            scannerEntity.OnlyPairs = scanner.OnlyPairs;
            scannerEntity.AutoAmount = scanner.AutoAmount;
            scannerEntity.ConfigExpire = scanner.ConfigExpire;
            scannerEntity.Elastic = scanner.Elastic;
            scannerEntity.OcNumber = scanner.OcNumber;
            scannerEntity.Turnover = scanner.Turnover;
            _context.Scanners.Update(scannerEntity);
            await _context.SaveChangesAsync();
        }
        await _bus.Send(new ScannerUpdateFromApiMessage { ScannerDtos = new List<ScannerDto> { scanner } });
        return true;
    }

    public async Task<bool> Delete(long id)
    {
        var scannerEntity = await _context.Scanners?.FirstOrDefaultAsync(c => c.Id == id);
        if (scannerEntity == null || (scannerEntity != null && scannerEntity.IsActive))
        {
            return false;
        }
        _context.Scanners.Remove(scannerEntity);
        await _context.SaveChangesAsync();
        
        return true;
    }

    public async Task<IEnumerable<ScannerDto>> GetScannerConfigsByUser(long userId)
    {
        var result = await _context.Scanners?.Where(b => b.Userid == userId).OrderBy(x => x.Title).ThenBy(x=>x.PositionSide).ThenBy(x=>x.OrderChange).ToListAsync() ?? new List<Scanner>();
        return result.Select(r => new ScannerDto
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
            AutoAmount = r.AutoAmount,
            ConfigExpire = r.ConfigExpire,
            Elastic = r.Elastic,
            OcNumber = r.OcNumber,
            Turnover = r.Turnover
        });        
    }

    public async Task<ScannerDto?> GetById(long id)
    {
        var scanner = await _context.Scanners?.FirstOrDefaultAsync(x => x.Id == id);
        if (null == scanner) return null;
        return new ScannerDto
        {
            Id = scanner.Id,
            UserId = scanner.Userid,
            PositionSide = scanner.PositionSide,
            Title = scanner.Title,
            OrderChange = scanner.OrderChange,
            IsActive = scanner.IsActive,
            Amount = scanner.Amount,
            OrderType = scanner.OrderType,
            AmountLimit = scanner.AmountLimit,
            AmountExpire = scanner.AmountExpire,
            BlackList = scanner.BlackList,
            OnlyPairs = scanner.OnlyPairs,
            AutoAmount = scanner.AutoAmount,
            ConfigExpire = scanner.ConfigExpire,
            Elastic = scanner.Elastic,
            OcNumber = scanner.OcNumber,
            Turnover = scanner.Turnover
        };        
    }

    public async Task<IEnumerable<ScannerDto>> GetAll()
    {
        List<ScannerDto> resultDto;        
        var result = await _context.Scanners?.ToListAsync() ?? new List<Scanner>();
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
        return resultDto;
    }

    public async Task<ScannerSettingDto?> GetScannerSettingByUser(long userId)
    {
        var r = await _context.ScannerSetting?.FirstOrDefaultAsync(b => b.Userid == userId);
        if (null == r) return new ScannerSettingDto();
        return new ScannerSettingDto
        {
            Id = r.Id,
            UserId = r.Userid,
            BlackList = r.BlackList,
            MaxOpen = r.MaxOpen
        };
    }

    public async Task<IEnumerable<ScannerSettingDto>> GetAllScannerSetting()
    {
        List<ScannerSettingDto> resultDto;
        var result = await _context.ScannerSetting?.ToListAsync() ?? new List<ScannerSetting>();
        resultDto = result.Select(r => new ScannerSettingDto
        {
            Id = r.Id,
            UserId = r.Userid,
            BlackList = r.BlackList,
            MaxOpen = r.MaxOpen
        }).ToList();
        return resultDto;
    }

    public async Task<bool> AddOrEditScannerSetting(ScannerSettingDto scannerSetting)
    {
        var scannerStEntity = await _context.ScannerSetting?.FirstOrDefaultAsync(c => c.Id == scannerSetting.Id);
        if (scannerStEntity == null)
        {
            //Add new
            var scannerAdd = new ScannerSetting
            {
                Userid = scannerSetting.UserId,
                BlackList = scannerSetting.BlackList,
                MaxOpen = scannerSetting.MaxOpen
            };
            
            _context.ScannerSetting.Add(scannerAdd);
            await _context.SaveChangesAsync();
        }
        else
        {
            //Edit
            scannerStEntity.BlackList = scannerSetting.BlackList;
            scannerStEntity.MaxOpen = scannerSetting.MaxOpen;
            _context.ScannerSetting.Update(scannerStEntity);
            await _context.SaveChangesAsync();
        }
        await _bus.Send(new ScannerSettingUpdateFromApiMessage { ScannerSettingDto = scannerSetting });
        return true;
    }

    public async Task<bool> SetScannerActiveStatus(long id, bool isActive)
    {
        try
        {
            var scanner = await _context.Scanners?.FirstOrDefaultAsync(x => x.Id == id);
            if (null == scanner) return false;
            scanner.IsActive = isActive;
            _context.Scanners.Update(scanner);
            await _context.SaveChangesAsync();
            await _bus.Send(new ScannerUpdateFromApiMessage { ScannerDtos = new List<ScannerDto> { scanner.ToDto() } });
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }
}