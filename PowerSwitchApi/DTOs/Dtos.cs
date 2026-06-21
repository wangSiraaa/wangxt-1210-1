using PowerSwitchApi.Models;

namespace PowerSwitchApi.DTOs;

public class PowerSwitchRequestDto
{
    public Guid Id { get; set; }
    public string RequestNo { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public RequestStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime RiskWindowStart { get; set; }
    public DateTime RiskWindowEnd { get; set; }
    public string? DutyManagerName { get; set; }
    public DateTime? DutyManagerFilledAt { get; set; }
    public string? EngineerName { get; set; }
    public DateTime? EngineerFilledAt { get; set; }
    public string? BusinessOwnerName { get; set; }
    public DateTime? BusinessConfirmedAt { get; set; }
    public bool IsLowPeakConfirmed { get; set; }
    public string? LowPeakRemark { get; set; }
    public bool DualPowerCheckPassed { get; set; }
    public DateTime? DualPowerCheckedAt { get; set; }
    public string? DualPowerCheckRemark { get; set; }
    public DateTime? ExecutionStartedAt { get; set; }
    public DateTime? ExecutionCompletedAt { get; set; }
    public string? RollbackReason { get; set; }
    public DateTime? RolledBackAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AffectedDeviceDto> AffectedDevices { get; set; } = new();
    public List<SwitchStepDto> SwitchSteps { get; set; } = new();
    public List<AlarmRecordDto> AlarmRecords { get; set; } = new();
    public List<BusinessImpactDto> BusinessImpacts { get; set; } = new();
}

public class CreateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime RiskWindowStart { get; set; }
    public DateTime RiskWindowEnd { get; set; }
}

public class UpdateRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime RiskWindowStart { get; set; }
    public DateTime RiskWindowEnd { get; set; }
}

public class AffectedDeviceDto
{
    public Guid Id { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? ImpactDescription { get; set; }
}

public class SaveAffectedDevicesDto
{
    public Guid RequestId { get; set; }
    public List<AffectedDeviceDto> Devices { get; set; } = new();
    public string DutyManagerName { get; set; } = string.Empty;
}

public class SwitchStepDto
{
    public Guid Id { get; set; }
    public int Sequence { get; set; }
    public string StepType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OperationDetail { get; set; }
    public int EstimatedDurationSeconds { get; set; }
    public StepStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? OperatorName { get; set; }
    public string? Remark { get; set; }
    public bool IsRollbackStep { get; set; }
}

public class SaveSwitchStepsDto
{
    public Guid RequestId { get; set; }
    public List<SwitchStepDto> Steps { get; set; } = new();
    public string EngineerName { get; set; } = string.Empty;
}

public class ConfirmLowPeakDto
{
    public Guid RequestId { get; set; }
    public string BusinessOwnerName { get; set; } = string.Empty;
    public string? LowPeakRemark { get; set; }
}

public class AlarmRecordDto
{
    public Guid Id { get; set; }
    public AlarmSeverity Severity { get; set; }
    public string SeverityText { get; set; } = string.Empty;
    public string AlarmMessage { get; set; } = string.Empty;
    public string? SourceDevice { get; set; }
    public DateTime AlarmTime { get; set; }
    public bool IsConfirmed { get; set; }
    public string? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? ConfirmRemark { get; set; }
}

public class ConfirmAlarmDto
{
    public Guid AlarmId { get; set; }
    public string ConfirmedBy { get; set; } = string.Empty;
    public string? ConfirmRemark { get; set; }
}

public class BusinessImpactDto
{
    public Guid Id { get; set; }
    public string BusinessSystemName { get; set; } = string.Empty;
    public string? SystemCode { get; set; }
    public int AffectedCabinetCount { get; set; }
    public int ConfirmStatus { get; set; }
    public string ConfirmStatusText { get; set; } = string.Empty;
    public string? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public string? ConfirmRemark { get; set; }
    public DateTime? ActualImpactStart { get; set; }
    public DateTime? ActualImpactEnd { get; set; }
    public string? ImpactDescription { get; set; }
    public string? RecoveryDetail { get; set; }
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class DualPowerCheckDto
{
    public Guid Id { get; set; }
    public string DeviceCode { get; set; } = string.Empty;
    public DeviceType DeviceType { get; set; }
    public bool RouteAPowered { get; set; }
    public bool RouteBPowered { get; set; }
    public bool BothRoutesHealthy { get; set; }
    public string? CheckRemark { get; set; }
    public string? CheckedBy { get; set; }
    public DateTime CheckedAt { get; set; }
}

public class SubmitDualPowerCheckDto
{
    public Guid RequestId { get; set; }
    public List<DualPowerCheckDto> Checks { get; set; } = new();
    public string CheckedBy { get; set; } = string.Empty;
    public string? Remark { get; set; }
}

public class DeviceTopologyDto
{
    public Guid Id { get; set; }
    public DeviceType DeviceType { get; set; }
    public string DeviceTypeText { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string? Location { get; set; }
    public Guid? ParentId { get; set; }
    public bool HasDualPower { get; set; }
    public string? RouteASource { get; set; }
    public string? RouteBSource { get; set; }
    public string? ConnectedBusinessSystems { get; set; }
    public List<DeviceTopologyDto> Children { get; set; } = new();
}

public class ExecuteStepDto
{
    public Guid StepId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
}

public class StartExecutionDto
{
    public Guid RequestId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
}

public class CompleteExecutionDto
{
    public Guid RequestId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
}

public class RollbackDto
{
    public Guid RequestId { get; set; }
    public string OperatorName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class BatchConfirmBusinessImpactDto
{
    public Guid RequestId { get; set; }
    public string ConfirmedBy { get; set; } = string.Empty;
    public List<BusinessImpactConfirmItemDto> Items { get; set; } = new();
}

public class BusinessImpactConfirmItemDto
{
    public Guid Id { get; set; }
    public int ConfirmStatus { get; set; }
    public string? ConfirmRemark { get; set; }
}
