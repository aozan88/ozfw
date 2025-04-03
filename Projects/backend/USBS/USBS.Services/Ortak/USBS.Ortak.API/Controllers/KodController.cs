using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;

namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KodController : ControllerBase
    {
        private readonly KodBusinessService _KodService;
        private readonly IValidator<KodDto> _KodValidator;

        public KodController(KodBusinessService KodService, IValidator<KodDto> KodValidator)
        {
            _KodService = KodService;
            _KodValidator = KodValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var Kods = await _KodService.GetAllActiveKodsAsync();
            return Ok(Kods);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var Kod = await _KodService.GetByIdAsync(id);
            if (Kod == null)
            {
                return NotFound(new { Message = $"Kod with ID {id} not found." });
            }

            return Ok(Kod);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] KodDto KodDto)
        {
            // Validate the incoming DTO
            var validationResult = await _KodValidator.ValidateAsync(KodDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var createdKod = await _KodService.AddKodAsync(KodDto);
            return CreatedAtAction(nameof(GetById), new { id = createdKod.KodTipiId }, createdKod);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] KodDto KodDto)
        {
            if (id != KodDto.KodTipiId)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _KodValidator.ValidateAsync(KodDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var updatedKod = await _KodService.UpdateKodAsync(id, KodDto);
            if (updatedKod == null)
            {
                return NotFound(new { Message = $"Kod with ID {id} not found for update." });
            }

            return Ok(updatedKod);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _KodService.DeleteKodAsync(id);
            if (!success)
            {
                return NotFound(new { Message = $"Kod with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
