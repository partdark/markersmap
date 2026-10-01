using Application.Services;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace markersAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MarkersController : ControllerBase
    {
        private readonly IMarkerService _service;

        public MarkersController(IMarkerService service)
        {
            _service = service;
        }

        private Guid? CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private bool IsAdmin => User.IsInRole("Admin");

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken ct = default)
        {
            var items = await _service.GetAllAsync(CurrentUserId, IsAdmin, ct);
            return Ok(items);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, CurrentUserId, IsAdmin, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("by-user/{userId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByUserId(Guid userId, CancellationToken ct = default)
        {
            var items = await _service.GetByUserIdAsync(userId, CurrentUserId, IsAdmin, ct);
            return Ok(items);
        }

        [HttpGet("by-category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByCategoryId(Guid categoryId, CancellationToken ct = default)
        {
            var items = await _service.GetByCategoryIdAsync(categoryId, CurrentUserId, IsAdmin, ct);
            return Ok(items);
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic(CancellationToken ct = default)
        {
            var items = await _service.GetPublicAsync(ct);
            return Ok(items);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MarkerDTO dto, CancellationToken ct = default)
        {
            try
            {
                if (CurrentUserId is null) return Unauthorized();
                var created = await _service.CreateAsync(dto, CurrentUserId.Value, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MarkerDTO dto, CancellationToken ct = default)
        {
            try
            {
                if (CurrentUserId is null) return Unauthorized();
                var updated = await _service.UpdateAsync(id, dto, CurrentUserId.Value, IsAdmin, ct);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
        {
            try
            {
                if (CurrentUserId is null) return Unauthorized();
                await _service.DeleteAsync(id, CurrentUserId.Value, IsAdmin, ct);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }
    }
}
