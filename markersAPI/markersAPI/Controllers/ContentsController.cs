using Application.Services;
using Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace markersAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ContentsController : ControllerBase
    {
        private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 МБ

        private readonly IContentService _service;

        public ContentsController(IContentService service)
        {
            _service = service;
        }

        private Guid? CurrentUserId =>
            Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

        private bool IsAdmin => User.IsInRole("Admin");

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
        {
            var item = await _service.GetByIdAsync(id, CurrentUserId, IsAdmin, ct);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpGet("by-marker/{markerId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetByMarkerId(Guid markerId, CancellationToken ct = default)
        {
            try
            {
                var items = await _service.GetByMarkerIdAsync(markerId, CurrentUserId, IsAdmin, ct);
                return Ok(items);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContentDTO dto, CancellationToken ct = default)
        {
            try
            {
                if (CurrentUserId is null) return Unauthorized();
                var created = await _service.CreateAsync(dto, CurrentUserId.Value, IsAdmin, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
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

        [HttpPost("upload")]
        [RequestSizeLimit(MaxFileSizeBytes)]
        public async Task<IActionResult> Upload([FromForm] Guid markerId, IFormFile? file, CancellationToken ct = default)
        {
            try
            {
                if (CurrentUserId is null) return Unauthorized();
                if (file == null || file.Length == 0)
                    return BadRequest(new { message = "Файл не передан" });

                var created = await _service.UploadAsync(markerId, file, CurrentUserId.Value, IsAdmin, ct);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (InvalidDataException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ContentDTO dto, CancellationToken ct = default)
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
