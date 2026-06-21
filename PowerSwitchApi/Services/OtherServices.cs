using AutoMapper;
using Microsoft.EntityFrameworkCore;
using PowerSwitchApi.Data;
using PowerSwitchApi.DTOs;
using PowerSwitchApi.Models;

namespace PowerSwitchApi.Services;

public class DeviceTopologyService : IDeviceTopologyService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public DeviceTopologyService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DeviceTopologyDto>> GetAllAsync()
    {
        var list = await _db.DeviceTopologies.ToListAsync();
        return _mapper.Map<IEnumerable<DeviceTopologyDto>>(list);
    }

    public async Task<IEnumerable<DeviceTopologyDto>> GetTreeAsync()
    {
        var all = await _db.DeviceTopologies.ToListAsync();
        var rootNodes = all.Where(n => n.ParentId == null).ToList();
        var result = new List<DeviceTopologyDto>();
        foreach (var root in rootNodes)
        {
            result.Add(BuildTree(root, all));
        }
        return result;
    }

    private DeviceTopologyDto BuildTree(DeviceTopology node, List<DeviceTopology> all)
    {
        var dto = _mapper.Map<DeviceTopologyDto>(node);
        var children = all.Where(c => c.ParentId == node.Id).ToList();
        foreach (var child in children)
        {
            dto.Children.Add(BuildTree(child, all));
        }
        return dto;
    }

    public async Task<IEnumerable<DeviceTopologyDto>> GetByTypeAsync(int deviceType)
    {
        var list = await _db.DeviceTopologies
            .Where(t => (int)t.DeviceType == deviceType)
            .ToListAsync();
        return _mapper.Map<IEnumerable<DeviceTopologyDto>>(list);
    }
}

public class SwitchStepService : ISwitchStepService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public SwitchStepService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<SwitchStepDto?> StartStepAsync(ExecuteStepDto dto)
    {
        var step = await _db.SwitchSteps.FindAsync(dto.StepId);
        if (step == null || step.Status != StepStatus.Pending) return null;

        step.Status = StepStatus.Executing;
        step.StartedAt = DateTime.UtcNow;
        step.OperatorName = dto.OperatorName;

        var request = await _db.PowerSwitchRequests.FindAsync(step.RequestId);
        if (request != null)
        {
            request.UpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return _mapper.Map<SwitchStepDto>(step);
    }

    public async Task<SwitchStepDto?> CompleteStepAsync(Guid stepId, string? remark)
    {
        var step = await _db.SwitchSteps.FindAsync(stepId);
        if (step == null || step.Status != StepStatus.Executing) return null;

        step.Status = StepStatus.Completed;
        step.CompletedAt = DateTime.UtcNow;
        step.Remark = remark;
        await _db.SaveChangesAsync();
        return _mapper.Map<SwitchStepDto>(step);
    }

    public async Task<SwitchStepDto?> SkipStepAsync(Guid stepId, string? remark)
    {
        var step = await _db.SwitchSteps.FindAsync(stepId);
        if (step == null) return null;

        step.Status = StepStatus.Skipped;
        step.Remark = remark;
        await _db.SaveChangesAsync();
        return _mapper.Map<SwitchStepDto>(step);
    }
}

public class AlarmService : IAlarmService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public AlarmService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AlarmRecordDto>> GetByRequestAsync(Guid requestId)
    {
        var list = await _db.AlarmRecords
            .Where(a => a.RequestId == requestId)
            .OrderByDescending(a => a.AlarmTime)
            .ToListAsync();
        return _mapper.Map<IEnumerable<AlarmRecordDto>>(list);
    }

    public async Task<AlarmRecordDto> CreateAsync(Guid requestId, AlarmRecordDto dto)
    {
        var entity = new AlarmRecord
        {
            Id = Guid.NewGuid(),
            RequestId = requestId,
            Severity = dto.Severity,
            AlarmMessage = dto.AlarmMessage,
            SourceDevice = dto.SourceDevice,
            AlarmTime = DateTime.UtcNow,
            IsConfirmed = false
        };
        _db.AlarmRecords.Add(entity);

        var request = await _db.PowerSwitchRequests.FindAsync(requestId);
        if (request != null && request.Status == RequestStatus.Executing)
        {
            request.Status = RequestStatus.AlarmsPending;
        }

        await _db.SaveChangesAsync();
        return _mapper.Map<AlarmRecordDto>(entity);
    }

    public async Task<AlarmRecordDto?> ConfirmAsync(ConfirmAlarmDto dto)
    {
        var alarm = await _db.AlarmRecords.FindAsync(dto.AlarmId);
        if (alarm == null) return null;

        alarm.IsConfirmed = true;
        alarm.ConfirmedBy = dto.ConfirmedBy;
        alarm.ConfirmedAt = DateTime.UtcNow;
        alarm.ConfirmRemark = dto.ConfirmRemark;

        var request = await _db.PowerSwitchRequests.FindAsync(alarm.RequestId);
        if (request != null && request.Status == RequestStatus.AlarmsPending)
        {
            var unconfirmed = await _db.AlarmRecords
                .AnyAsync(a => a.RequestId == alarm.RequestId && !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
            if (!unconfirmed)
            {
                request.Status = RequestStatus.Executing;
            }
        }

        await _db.SaveChangesAsync();
        return _mapper.Map<AlarmRecordDto>(alarm);
    }

    public async Task<bool> HasUnconfirmedCriticalAsync(Guid requestId)
    {
        return await _db.AlarmRecords
            .AnyAsync(a => a.RequestId == requestId && !a.IsConfirmed && a.Severity >= AlarmSeverity.Warning);
    }
}

public class BusinessImpactService : IBusinessImpactService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public BusinessImpactService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<BusinessImpactDto>> GetByRequestAsync(Guid requestId)
    {
        var list = await _db.BusinessImpacts
            .Where(i => i.RequestId == requestId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<BusinessImpactDto>>(list);
    }

    public async Task<BusinessImpactDto?> VerifyAsync(Guid id, string verifiedBy)
    {
        var entity = await _db.BusinessImpacts.FindAsync(id);
        if (entity == null) return null;

        entity.VerifiedBy = verifiedBy;
        entity.VerifiedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return _mapper.Map<BusinessImpactDto>(entity);
    }
}

public class DualPowerCheckService : IDualPowerCheckService
{
    private readonly AppDbContext _db;
    private readonly IMapper _mapper;

    public DualPowerCheckService(AppDbContext db, IMapper mapper)
    {
        _db = db;
        _mapper = mapper;
    }

    public async Task<IEnumerable<DualPowerCheckDto>> GetByRequestAsync(Guid requestId)
    {
        var list = await _db.DualPowerCheckRecords
            .Where(c => c.RequestId == requestId)
            .ToListAsync();
        return _mapper.Map<IEnumerable<DualPowerCheckDto>>(list);
    }

    public async Task<PowerSwitchRequestDto?> SubmitCheckAsync(SubmitDualPowerCheckDto dto)
    {
        var request = await _db.PowerSwitchRequests
            .Include(r => r.DualPowerCheckRecords)
            .FirstOrDefaultAsync(r => r.Id == dto.RequestId);
        if (request == null) return null;

        _db.DualPowerCheckRecords.RemoveRange(request.DualPowerCheckRecords);

        bool allPassed = true;
        foreach (var checkDto in dto.Checks)
        {
            var check = _mapper.Map<DualPowerCheckRecord>(checkDto);
            check.Id = Guid.NewGuid();
            check.RequestId = dto.RequestId;
            check.CheckedBy = dto.CheckedBy;
            check.CheckedAt = DateTime.UtcNow;
            check.BothRoutesHealthy = check.RouteAPowered && check.RouteBPowered;
            if (!check.BothRoutesHealthy) allPassed = false;
            _db.DualPowerCheckRecords.Add(check);
        }

        request.DualPowerCheckPassed = allPassed;
        request.DualPowerCheckedAt = DateTime.UtcNow;
        request.DualPowerCheckRemark = dto.Remark;
        if (allPassed && request.Status == RequestStatus.BusinessConfirmed)
            request.Status = RequestStatus.DualPowerChecked;
        if (request.Status >= RequestStatus.DualPowerChecked && !allPassed)
            request.Status = RequestStatus.BusinessConfirmed;
        request.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return await GetFullRequestById(dto.RequestId);
    }

    private async Task<PowerSwitchRequestDto?> GetFullRequestById(Guid requestId)
    {
        var entity = await _db.PowerSwitchRequests
            .Include(r => r.AffectedDevices)
            .Include(r => r.SwitchSteps)
            .Include(r => r.AlarmRecords)
            .Include(r => r.RollbackRecords)
            .Include(r => r.BusinessImpacts)
            .Include(r => r.DualPowerCheckRecords)
            .FirstOrDefaultAsync(r => r.Id == requestId);
        return entity == null ? null : _mapper.Map<PowerSwitchRequestDto>(entity);
    }
}
