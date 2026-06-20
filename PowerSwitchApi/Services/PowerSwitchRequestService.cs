using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PowerSwitchApi.Data;
using PowerSwitchApi.DTOs;
using PowerSwitchApi.Models;

namespace PowerSwitchApi.Services;

public class PowerSwitchRequestService : IPowerSwitchRequestService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public PowerSwitchRequestService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PowerSwitchRequestDto>> GetAllAsync()
    {
        var list = await _db.PowerSwitchRequests
            .Include(r => r.AffectedDevices)
            .Include(r => r.SwitchSteps)
            .Include(r => r.AlarmRecords)
            .Include(r => r.BusinessImpacts)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        return _mapper.Map<IEnumerable<PowerSwitchRequestDto>>(list);
    }

    public async Task<PowerSwitchRequestDto?> GetByIdAsync(Guid id)
    {
        var entity = await _db.PowerSwitchRequests
            .Include(r => r.AffectedDevices)
            .Include(r => r.SwitchSteps)
            .Include(r => r.AlarmRecords)
            .Include(r => r.RollbackRecords)
            .Include(r => r.BusinessImpacts)
            .Include(r => r.DualPowerCheckRecords)
            .FirstOrDefaultAsync(r => r.Id == id);
        return entity == null ? null : _mapper.Map<PowerSwitchRequestDto>(entity);
    }

    public async Task<PowerSwitchRequestDto> CreateAsync(CreateRequestDto dto)
    {
        var entity = _mapper.Map<PowerSwitchRequest>(dto);
        entity.Id = Guid.NewGuid();
        entity.RequestNo = $"PS{DateTime.Now:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
        entity.Status = RequestStatus.Draft;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        _db.PowerSwitchRequests.Add(entity);
        await _db.SaveChangesAsync();
        return _mapper.Map<PowerSwitchRequestDto>(entity);
    }

    public async Task<PowerSwitchRequestDto?> UpdateAsync(Guid id, UpdateRequestDto dto)
    {
        var entity = await _db.PowerSwitchRequests.FindAsync(id);
        if (entity == null) return null;
        _mapper.Map(dto, entity);
        entity.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return _mapper.Map<PowerSwitchRequestDto>(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _db.PowerSwitchRequests.FindAsync(id);
        if (entity == null) return false;
        _db.PowerSwitchRequests.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<PowerSwitchRequestDto?> SaveAffectedDevicesAsync(SaveAffectedDevicesDto dto)
    {
        var request = await _db.PowerSwitchRequests
            .Include(r => r.AffectedDevices)
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (request == null) return null;

        _db.AffectedDevices.RemoveRange(request.AffectedDevices);
        foreach (var devDto in dto.Devices)
        {
            var dev = _mapper.Map<AffectedDevice>(devDto);
            dev.Id = Guid.NewGuid();
            dev.RequestId = dto.RequestId;
            request.AffectedDevices.Add(dev);
        }

        request.DutyManagerName = dto.DutyManagerName;
        request.DutyManagerFilledAt = DateTime.UtcNow;
        if (request.Status == RequestStatus.Draft)
            request.Status = RequestStatus.DutyManagerFilled;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    public async Task<PowerSwitchRequestDto?> SaveSwitchStepsAsync(SaveSwitchStepsDto dto)
    {
        var request = await _db.PowerSwitchRequests
            .Include(r => r.SwitchSteps)
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (request == null) return null;

        _db.SwitchSteps.RemoveRange(request.SwitchSteps);
        foreach (var stepDto in dto.Steps)
        {
            var step = _mapper.Map<SwitchStep>(stepDto);
            step.Id = Guid.NewGuid();
            step.RequestId = dto.RequestId;
            step.Status = StepStatus.Pending;
            request.SwitchSteps.Add(step);
        }

        request.EngineerName = dto.EngineerName;
        request.EngineerFilledAt = DateTime.UtcNow;
        if (request.Status == RequestStatus.DutyManagerFilled)
            request.Status = RequestStatus.EngineerFilled;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    public async Task<PowerSwitchRequestDto?> ConfirmLowPeakAsync(ConfirmLowPeakDto dto)
    {
        var request = await _db.PowerSwitchRequests.FindAsync(dto.RequestId);
        if (request == null) return null;

        request.BusinessOwnerName = dto.BusinessOwnerName;
        request.BusinessConfirmedAt = DateTime.UtcNow;
        request.IsLowPeakConfirmed = true;
        request.LowPeakRemark = dto.LowPeakRemark;
        if (request.Status == RequestStatus.EngineerFilled)
            request.Status = RequestStatus.BusinessConfirmed;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    public async Task<bool> CanExecuteAsync(Guid requestId)
    {
        var request = await _db.PowerSwitchRequests
            .Include(r => r.DualPowerCheckRecords)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null) return false;

        if (!request.DualPowerCheckPassed) return false;
        if (!request.IsLowPeakConfirmed) return false;
        if (request.Status < RequestStatus.DualPowerChecked) return false;

        var unconfirmedAlarms = await _db.AlarmRecords
            .AnyAsync(a => a.RequestId == requestId && !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
        if (unconfirmedAlarms) return false;

        return true;
    }

    public async Task<PowerSwitchRequestDto?> StartExecutionAsync(StartExecutionDto dto)
    {
        if (!await CanExecuteAsync(dto.RequestId)) return null;

        var request = await _db.PowerSwitchRequests.FindAsync(dto.RequestId);
        if (request == null) return null;

        request.Status = RequestStatus.Executing;
        request.ExecutionStartedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    public async Task<PowerSwitchRequestDto?> CompleteExecutionAsync(CompleteExecutionDto dto)
    {
        var request = await _db.PowerSwitchRequests
            .Include(r => r.AlarmRecords)
            .Include(r => r.SwitchSteps)
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (request == null) return null;

        var unconfirmedCritical = request.AlarmRecords
            .Any(a => !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
        if (unconfirmedCritical) return null;

        var incompleteSteps = request.SwitchSteps
            .Where(s => !s.IsRollbackStep && s.Status != StepStatus.Completed && s.Status != StepStatus.Skipped)
            .ToList();
        if (incompleteSteps.Any()) return null;

        request.Status = RequestStatus.Completed;
        request.ExecutionCompletedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await GenerateBusinessImpactList(request);

        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    public async Task<PowerSwitchRequestDto?> RollbackAsync(RollbackDto dto)
    {
        var request = await _db.PowerSwitchRequests.FindAsync(dto.RequestId);
        if (request == null) return null;

        request.Status = RequestStatus.RolledBack;
        request.RollbackReason = dto.Reason;
        request.RolledBackAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetByIdAsync(dto.RequestId);
    }

    private async Task GenerateBusinessImpactList(PowerSwitchRequest request)
    {
        var affectedCabinets = await _db.AffectedDevices
            .Where(d => d.RequestId == request.Id && d.DeviceType == DeviceType.Cabinet)
            .ToListAsync();

        var topologyDict = await _db.DeviceTopologies
            .Where(t => t.DeviceType == DeviceType.Cabinet)
            .ToDictionaryAsync(t => t.DeviceCode);

        var systemDict = new Dictionary<string, BusinessImpact>();

        foreach (var cab in affectedCabinets)
        {
            if (topologyDict.TryGetValue(cab.DeviceCode, out var topo)
                && !string.IsNullOrEmpty(topo.ConnectedBusinessSystems))
            {
                var systems = topo.ConnectedBusinessSystems.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var sys in systems)
                {
                    var key = sys.Trim();
                    if (!systemDict.ContainsKey(key))
                    {
                        systemDict[key] = new BusinessImpact
                        {
                            Id = Guid.NewGuid(),
                            RequestId = request.Id,
                            BusinessSystemName = key,
                            SystemCode = key,
                            AffectedCabinetCount = 0,
                            ActualImpactStart = request.ExecutionStartedAt,
                            ActualImpactEnd = request.ExecutionCompletedAt,
                            ImpactDescription = "供电切换影响",
                            RecoveryDetail = "已恢复双路供电"
                        };
                    }
                    systemDict[key].AffectedCabinetCount++;
                }
            }
        }

        foreach (var bi in systemDict.Values)
        {
            _db.BusinessImpacts.Add(bi);
        }
    }
}
