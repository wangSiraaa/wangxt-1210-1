using Microsoft.AspNetCore.Mvc;
using PowerSwitchApi.DTOs;
using PowerSwitchApi.Services;

namespace PowerSwitchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PowerSwitchRequestsController : ControllerBase
{
    private readonly IPowerSwitchRequestService _service;

    public PowerSwitchRequestsController(IPowerSwitchRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PowerSwitchRequestDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PowerSwitchRequestDto>> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<PowerSwitchRequestDto>> Create([FromBody] CreateRequestDto dto)
    {
        var result = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PowerSwitchRequestDto>> Update(Guid id, [FromBody] UpdateRequestDto dto)
    {
        var result = await _service.UpdateAsync(id, dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("affected-devices")]
    public async Task<ActionResult<PowerSwitchRequestDto>> SaveAffectedDevices([FromBody] SaveAffectedDevicesDto dto)
    {
        var result = await _service.SaveAffectedDevicesAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("switch-steps")]
    public async Task<ActionResult<PowerSwitchRequestDto>> SaveSwitchSteps([FromBody] SaveSwitchStepsDto dto)
    {
        var result = await _service.SaveSwitchStepsAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost("confirm-lowpeak")]
    public async Task<ActionResult<PowerSwitchRequestDto>> ConfirmLowPeak([FromBody] ConfirmLowPeakDto dto)
    {
        var result = await _service.ConfirmLowPeakAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("{id}/can-execute")]
    public async Task<ActionResult<bool>> CanExecute(Guid id)
    {
        var result = await _service.CanExecuteAsync(id);
        return Ok(result);
    }

    [HttpPost("start-execution")]
    public async Task<ActionResult<PowerSwitchRequestDto>> StartExecution([FromBody] StartExecutionDto dto)
    {
        var result = await _service.StartExecutionAsync(dto);
        if (result == null) return BadRequest("前置校验未通过，无法开始执行");
        return Ok(result);
    }

    [HttpPost("complete-execution")]
    public async Task<ActionResult<PowerSwitchRequestDto>> CompleteExecution([FromBody] CompleteExecutionDto dto)
    {
        var result = await _service.CompleteExecutionAsync(dto);
        if (result == null) return BadRequest("存在未确认告警或未完成步骤");
        return Ok(result);
    }

    [HttpPost("rollback")]
    public async Task<ActionResult<PowerSwitchRequestDto>> Rollback([FromBody] RollbackDto dto)
    {
        var result = await _service.RollbackAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class DeviceTopologyController : ControllerBase
{
    private readonly IDeviceTopologyService _service;

    public DeviceTopologyController(IDeviceTopologyService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<DeviceTopologyDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("tree")]
    public async Task<ActionResult<IEnumerable<DeviceTopologyDto>>> GetTree()
    {
        return Ok(await _service.GetTreeAsync());
    }

    [HttpGet("by-type/{deviceType}")]
    public async Task<ActionResult<IEnumerable<DeviceTopologyDto>>> GetByType(int deviceType)
    {
        return Ok(await _service.GetByTypeAsync(deviceType));
    }
}

[ApiController]
[Route("api/[controller]")]
public class SwitchStepsController : ControllerBase
{
    private readonly ISwitchStepService _service;

    public SwitchStepsController(ISwitchStepService service)
    {
        _service = service;
    }

    [HttpPost("start")]
    public async Task<ActionResult<SwitchStepDto>> StartStep([FromBody] ExecuteStepDto dto)
    {
        var result = await _service.StartStepAsync(dto);
        if (result == null) return BadRequest();
        return Ok(result);
    }

    [HttpPost("{id}/complete")]
    public async Task<ActionResult<SwitchStepDto>> CompleteStep(Guid id, [FromBody] CompleteStepBody body)
    {
        var result = await _service.CompleteStepAsync(id, body.Remark);
        if (result == null) return BadRequest();
        return Ok(result);
    }

    [HttpPost("{id}/skip")]
    public async Task<ActionResult<SwitchStepDto>> SkipStep(Guid id, [FromBody] CompleteStepBody body)
    {
        var result = await _service.SkipStepAsync(id, body.Remark);
        if (result == null) return BadRequest();
        return Ok(result);
    }
}

public class CompleteStepBody
{
    public string? Remark { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AlarmsController : ControllerBase
{
    private readonly IAlarmService _service;

    public AlarmsController(IAlarmService service)
    {
        _service = service;
    }

    [HttpGet("by-request/{requestId}")]
    public async Task<ActionResult<IEnumerable<AlarmRecordDto>>> GetByRequest(Guid requestId)
    {
        return Ok(await _service.GetByRequestAsync(requestId));
    }

    [HttpPost("request/{requestId}")]
    public async Task<ActionResult<AlarmRecordDto>> Create(Guid requestId, [FromBody] AlarmRecordDto dto)
    {
        var result = await _service.CreateAsync(requestId, dto);
        return Ok(result);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult<AlarmRecordDto>> Confirm([FromBody] ConfirmAlarmDto dto)
    {
        var result = await _service.ConfirmAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpGet("by-request/{requestId}/has-unconfirmed")]
    public async Task<ActionResult<bool>> HasUnconfirmedCritical(Guid requestId)
    {
        return Ok(await _service.HasUnconfirmedCriticalAsync(requestId));
    }
}

[ApiController]
[Route("api/[controller]")]
public class BusinessImpactsController : ControllerBase
{
    private readonly IBusinessImpactService _service;

    public BusinessImpactsController(IBusinessImpactService service)
    {
        _service = service;
    }

    [HttpGet("by-request/{requestId}")]
    public async Task<ActionResult<IEnumerable<BusinessImpactDto>>> GetByRequest(Guid requestId)
    {
        return Ok(await _service.GetByRequestAsync(requestId));
    }

    [HttpPost("{id}/verify")]
    public async Task<ActionResult<BusinessImpactDto>> Verify(Guid id, [FromBody] VerifyBody body)
    {
        var result = await _service.VerifyAsync(id, body.VerifiedBy);
        if (result == null) return NotFound();
        return Ok(result);
    }
}

public class VerifyBody
{
    public string VerifiedBy { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class DualPowerChecksController : ControllerBase
{
    private readonly IDualPowerCheckService _service;

    public DualPowerChecksController(IDualPowerCheckService service)
    {
        _service = service;
    }

    [HttpGet("by-request/{requestId}")]
    public async Task<ActionResult<IEnumerable<DualPowerCheckDto>>> GetByRequest(Guid requestId)
    {
        return Ok(await _service.GetByRequestAsync(requestId));
    }

    [HttpPost]
    public async Task<ActionResult<PowerSwitchRequestDto>> Submit([FromBody] SubmitDualPowerCheckDto dto)
    {
        var result = await _service.SubmitCheckAsync(dto);
        if (result == null) return NotFound();
        return Ok(result);
    }
}
