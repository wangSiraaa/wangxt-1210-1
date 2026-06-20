using PowerSwitchApi.DTOs;

namespace PowerSwitchApi.Services;

public interface IPowerSwitchRequestService
{
    Task<IEnumerable<PowerSwitchRequestDto>> GetAllAsync();
    Task<PowerSwitchRequestDto?> GetByIdAsync(Guid id);
    Task<PowerSwitchRequestDto> CreateAsync(CreateRequestDto dto);
    Task<PowerSwitchRequestDto?> UpdateAsync(Guid id, UpdateRequestDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<PowerSwitchRequestDto?> SaveAffectedDevicesAsync(SaveAffectedDevicesDto dto);
    Task<PowerSwitchRequestDto?> SaveSwitchStepsAsync(SaveSwitchStepsDto dto);
    Task<PowerSwitchRequestDto?> ConfirmLowPeakAsync(ConfirmLowPeakDto dto);
    Task<PowerSwitchRequestDto?> StartExecutionAsync(StartExecutionDto dto);
    Task<PowerSwitchRequestDto?> CompleteExecutionAsync(CompleteExecutionDto dto);
    Task<PowerSwitchRequestDto?> RollbackAsync(RollbackDto dto);
    Task<bool> CanExecuteAsync(Guid requestId);
}
