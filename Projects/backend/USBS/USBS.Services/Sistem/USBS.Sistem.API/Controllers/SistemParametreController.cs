using Microsoft.AspNetCore.Mvc;
using Sistem.Application.Business;
using Sistem.Application.DtoModels;
using FluentValidation;

namespace Sistem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemParametreController : ControllerBase
    {
        private readonly SistemParametreServis _SistemParametreService;
        private readonly IValidator<SistemParametreDto> _SistemParametreValidator;

        public SistemParametreController(SistemParametreServis SistemParametreService, IValidator<SistemParametreDto> SistemParametreValidator)
        {
            _SistemParametreService = SistemParametreService;
            _SistemParametreValidator = SistemParametreValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var SistemParametres = await _SistemParametreService.GetAllActiveSistemParametresAsync();
            return Ok(SistemParametres);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var SistemParametre = await _SistemParametreService.GetByIdAsync(id);
            if (SistemParametre == null)
            {
                return NotFound(new { Message = $"SistemParametre with ID {id} not found." });
            }

            return Ok(SistemParametre);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SistemParametreDto SistemParametreDto)
        {
            // Validate the incoming DTO
            var validationResult = await _SistemParametreValidator.ValidateAsync(SistemParametreDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var createdSistemParametre = await _SistemParametreService.AddSistemParametreAsync(SistemParametreDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSistemParametre.SistemParametreTipiId }, createdSistemParametre);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SistemParametreDto SistemParametreDto)
        {
            if (id != SistemParametreDto.SistemParametreTipiId)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _SistemParametreValidator.ValidateAsync(SistemParametreDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var updatedSistemParametre = await _SistemParametreService.UpdateSistemParametreAsync(id, SistemParametreDto);
            if (updatedSistemParametre == null)
            {
                return NotFound(new { Message = $"SistemParametre with ID {id} not found for update." });
            }

            return Ok(updatedSistemParametre);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _SistemParametreService.DeleteSistemParametreAsync(id);
            if (!success)
            {
                return NotFound(new { Message = $"SistemParametre with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
