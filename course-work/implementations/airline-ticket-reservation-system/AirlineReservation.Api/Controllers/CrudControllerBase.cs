using AirlineReservation.ApplicationServices.Interfaces;
using AirlineReservation.Contracts.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AirlineReservation.Api.Controllers;

[ApiController]
[Authorize]
public abstract class CrudControllerBase<TFilter, TCreate, TUpdate, TResponse> : ControllerBase
    where TFilter : PagedRequest
{
    private readonly ICrudService<TFilter, TCreate, TUpdate, TResponse> _service;

    protected CrudControllerBase(ICrudService<TFilter, TCreate, TUpdate, TResponse> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<TResponse>>> GetAll([FromQuery] TFilter filter, CancellationToken cancellationToken)
        => Ok(await _service.GetAllAsync(filter, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TResponse>> Create([FromBody] TCreate request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        var id = result?.GetType().GetProperty("Id")?.GetValue(result);
        return CreatedAtAction(nameof(GetById), new { id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TResponse>> Update(int id, [FromBody] TUpdate request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}/force")]
    public async Task<IActionResult> ForceDelete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _service.ForceDeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
