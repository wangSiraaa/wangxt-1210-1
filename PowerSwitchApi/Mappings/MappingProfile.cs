using AutoMapper;
using PowerSwitchApi.DTOs;
using PowerSwitchApi.Models;

namespace PowerSwitchApi.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<PowerSwitchRequest, PowerSwitchRequestDto>()
            .ForMember(d => d.StatusText, opt => opt.MapFrom(s => GetStatusText(s.Status)))
            .ForMember(d => d.AffectedDevices, opt => opt.MapFrom(s => s.AffectedDevices))
            .ForMember(d => d.SwitchSteps, opt => opt.MapFrom(s => s.SwitchSteps.OrderBy(x => x.Sequence)))
            .ForMember(d => d.AlarmRecords, opt => opt.MapFrom(s => s.AlarmRecords.OrderByDescending(x => x.AlarmTime)))
            .ForMember(d => d.BusinessImpacts, opt => opt.MapFrom(s => s.BusinessImpacts));

        CreateMap<CreateRequestDto, PowerSwitchRequest>();
        CreateMap<UpdateRequestDto, PowerSwitchRequest>();

        CreateMap<AffectedDevice, AffectedDeviceDto>().ReverseMap();

        CreateMap<SwitchStep, SwitchStepDto>()
            .ForMember(d => d.StatusText, opt => opt.MapFrom(s => GetStepStatusText(s.Status)));
        CreateMap<SwitchStepDto, SwitchStep>();

        CreateMap<AlarmRecord, AlarmRecordDto>()
            .ForMember(d => d.SeverityText, opt => opt.MapFrom(s => GetSeverityText(s.Severity)));

        CreateMap<BusinessImpact, BusinessImpactDto>()
            .ForMember(d => d.ConfirmStatusText, opt => opt.MapFrom(s => GetConfirmStatusText(s.ConfirmStatus)));
        CreateMap<BusinessImpactDto, BusinessImpact>();
        CreateMap<DualPowerCheckRecord, DualPowerCheckDto>().ReverseMap();

        CreateMap<DeviceTopology, DeviceTopologyDto>()
            .ForMember(d => d.DeviceTypeText, opt => opt.MapFrom(s => GetDeviceTypeText(s.DeviceType)))
            .ForMember(d => d.Children, opt => opt.MapFrom(s => s.Children));
    }

    private static string GetStatusText(RequestStatus status) => status switch
    {
        RequestStatus.Draft => "草稿",
        RequestStatus.DutyManagerFilled => "值班经理已登记",
        RequestStatus.EngineerFilled => "强电工程师已录入",
        RequestStatus.BusinessConfirmed => "业务负责人已确认",
        RequestStatus.DualPowerChecked => "双路供电已校验",
        RequestStatus.ReadyForExecution => "待执行",
        RequestStatus.Executing => "执行中",
        RequestStatus.AlarmsPending => "告警待确认",
        RequestStatus.Recovering => "恢复中",
        RequestStatus.Completed => "已完成",
        RequestStatus.RolledBack => "已回退",
        RequestStatus.Cancelled => "已取消",
        _ => "未知"
    };

    private static string GetStepStatusText(StepStatus status) => status switch
    {
        StepStatus.Pending => "待执行",
        StepStatus.Executing => "执行中",
        StepStatus.Completed => "已完成",
        StepStatus.Skipped => "已跳过",
        StepStatus.Failed => "失败",
        _ => "未知"
    };

    private static string GetSeverityText(AlarmSeverity severity) => severity switch
    {
        AlarmSeverity.Info => "信息",
        AlarmSeverity.Warning => "警告",
        AlarmSeverity.Critical => "严重",
        _ => "未知"
    };

    private static string GetDeviceTypeText(DeviceType type) => type switch
    {
        DeviceType.UPS => "UPS",
        DeviceType.BusBar => "母线",
        DeviceType.Cabinet => "机柜",
        DeviceType.ATS => "ATS",
        DeviceType.PDU => "PDU",
        _ => "未知"
    };

    private static string GetConfirmStatusText(BusinessConfirmStatus status) => status switch
    {
        BusinessConfirmStatus.Unconfirmed => "未确认",
        BusinessConfirmStatus.Confirmed => "已确认",
        BusinessConfirmStatus.ToDiscuss => "待沟通",
        _ => "未知"
    };
}
