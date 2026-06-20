using PowerSwitchApi.DTOs;

namespace PowerSwitchApi.Services;

public interface IDeviceTopologyService
{
    Task<IEnumerable<DeviceTopologyDto>> GetAllAsync();
    Task<IEnumerable<DeviceTopologyDto>> GetTreeAsync();
    Task<IEnumerable<DeviceTopologyDto>> GetByTypeAsync(int deviceType);
}

public interface ISwitchStepService
{
    Task<SwitchStepDto?> StartStepAsync(ExecuteStepDto dto);
    Task<SwitchStepDto?> CompleteStepAsync(Guid stepId, string? remark);
    Task<SwitchStepDto?> SkipStepAsync(Guid stepId, string? remark);
}

public interface IAlarmService
{
    Task<IEnumerable<AlarmRecordDto>> GetByRequestAsync(Guid requestId);
    Task<AlarmRecordDto> CreateAsync(Guid requestId, AlarmRecordDto dto);
    Task<AlarmRecordDto?> ConfirmAsync(ConfirmAlarmDto dto);
    Task<bool> HasUnconfirmedCriticalAsync(Guid requestId);
}

public interface IBusinessImpactService
{
    Task<IEnumerable<BusinessImpactDto>> GetByRequestAsync(Guid requestId);
    Task<BusinessImpactDto?> VerifyAsync(Guid id, string verifiedBy);
}

public interface IDualPowerCheckService
{
    Task<IEnumerable<DualPowerCheckDto>> GetByRequestAsync(Guid requestId);
    Task<PowerSwitchRequestDto?> SubmitCheckAsync(SubmitDualPowerCheckDto dto);
}
