using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PowerSwitchApi.Models;

public enum RequestStatus
{
    Draft = 0,
    DutyManagerFilled = 10,
    EngineerFilled = 20,
    BusinessConfirmed = 30,
    DualPowerChecked = 40,
    ReadyForExecution = 50,
    Executing = 60,
    AlarmsPending = 65,
    Recovering = 70,
    Completed = 80,
    RolledBack = 90,
    Cancelled = 100
}

public enum DeviceType
{
    UPS = 1,
    BusBar = 2,
    Cabinet = 3,
    ATS = 4,
    PDU = 5
}

public enum AlarmSeverity
{
    Info = 1,
    Warning = 2,
    Critical = 3
}

public enum StepStatus
{
    Pending = 0,
    Executing = 1,
    Completed = 2,
    Skipped = 3,
    Failed = 4
}

public class PowerSwitchRequest
{
    [Key]
    public Guid Id { get; set; }

    [MaxLength(50)]
    public string RequestNo { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public RequestStatus Status { get; set; }

    public DateTime RiskWindowStart { get; set; }
    public DateTime RiskWindowEnd { get; set; }

    [MaxLength(100)]
    public string? DutyManagerName { get; set; }
    public DateTime? DutyManagerFilledAt { get; set; }

    [MaxLength(100)]
    public string? EngineerName { get; set; }
    public DateTime? EngineerFilledAt { get; set; }

    [MaxLength(100)]
    public string? BusinessOwnerName { get; set; }
    public DateTime? BusinessConfirmedAt { get; set; }

    public bool IsLowPeakConfirmed { get; set; }
    [MaxLength(500)]
    public string? LowPeakRemark { get; set; }

    public bool DualPowerCheckPassed { get; set; }
    public DateTime? DualPowerCheckedAt { get; set; }
    [MaxLength(500)]
    public string? DualPowerCheckRemark { get; set; }

    public DateTime? ExecutionStartedAt { get; set; }
    public DateTime? ExecutionCompletedAt { get; set; }

    [MaxLength(2000)]
    public string? RollbackReason { get; set; }
    public DateTime? RolledBackAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<AffectedDevice> AffectedDevices { get; set; } = new();
    public List<SwitchStep> SwitchSteps { get; set; } = new();
    public List<AlarmRecord> AlarmRecords { get; set; } = new();
    public List<RollbackRecord> RollbackRecords { get; set; } = new();
    public List<BusinessImpact> BusinessImpacts { get; set; } = new();
    public List<DualPowerCheckRecord> DualPowerCheckRecords { get; set; } = new();
}

public class AffectedDevice
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    public DeviceType DeviceType { get; set; }

    [MaxLength(100)]
    public string DeviceCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DeviceName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }

    [MaxLength(1000)]
    public string? ImpactDescription { get; set; }
}

public class SwitchStep
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    public int Sequence { get; set; }

    [MaxLength(100)]
    public string StepType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? OperationDetail { get; set; }

    public int EstimatedDurationSeconds { get; set; }

    public StepStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? OperatorName { get; set; }

    [MaxLength(1000)]
    public string? Remark { get; set; }

    public bool IsRollbackStep { get; set; }
}

public class AlarmRecord
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    public AlarmSeverity Severity { get; set; }

    [MaxLength(500)]
    public string AlarmMessage { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? SourceDevice { get; set; }

    public DateTime AlarmTime { get; set; }

    public bool IsConfirmed { get; set; }
    [MaxLength(100)]
    public string? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    [MaxLength(1000)]
    public string? ConfirmRemark { get; set; }
}

public class RollbackRecord
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    public int Sequence { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? OperationDetail { get; set; }

    public StepStatus Status { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    [MaxLength(100)]
    public string? OperatorName { get; set; }
}

public class BusinessImpact
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    [MaxLength(200)]
    public string BusinessSystemName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? SystemCode { get; set; }

    public int AffectedCabinetCount { get; set; }

    public DateTime? ActualImpactStart { get; set; }
    public DateTime? ActualImpactEnd { get; set; }

    [MaxLength(1000)]
    public string? ImpactDescription { get; set; }

    [MaxLength(1000)]
    public string? RecoveryDetail { get; set; }

    [MaxLength(100)]
    public string? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
}

public class DualPowerCheckRecord
{
    [Key]
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public PowerSwitchRequest? Request { get; set; }

    [MaxLength(100)]
    public string DeviceCode { get; set; } = string.Empty;

    public DeviceType DeviceType { get; set; }

    public bool RouteAPowered { get; set; }
    public bool RouteBPowered { get; set; }
    public bool BothRoutesHealthy { get; set; }

    [MaxLength(1000)]
    public string? CheckRemark { get; set; }

    [MaxLength(100)]
    public string? CheckedBy { get; set; }
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

public class DeviceTopology
{
    [Key]
    public Guid Id { get; set; }

    public DeviceType DeviceType { get; set; }

    [MaxLength(100)]
    public string DeviceCode { get; set; } = string.Empty;

    [MaxLength(200)]
    public string DeviceName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Location { get; set; }

    public Guid? ParentId { get; set; }
    public DeviceTopology? Parent { get; set; }
    public List<DeviceTopology> Children { get; set; } = new();

    public bool HasDualPower { get; set; }
    [MaxLength(100)]
    public string? RouteASource { get; set; }
    [MaxLength(100)]
    public string? RouteBSource { get; set; }

    [MaxLength(200)]
    public string? ConnectedBusinessSystems { get; set; }
}
