using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;

namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemTanimController : ControllerBase
    {
        private readonly SistemTanimServis _SistemTanimServis;
        private readonly IValidator<SistemTanimDto> _SistemTanimValidator;

        public SistemTanimController(SistemTanimServis SistemTanimService, IValidator<SistemTanimDto> SistemTanimValidator)
        {
            _SistemTanimServis = SistemTanimService;
            _SistemTanimValidator = SistemTanimValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var SistemTanims = await _SistemTanimServis.GetAllActiveSistemTanimsAsync();
            return Ok(SistemTanims);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var SistemTanim = await _SistemTanimServis.GetByIdAsync(id);
            if (SistemTanim == null)
            {
                return NotFound(new { Message = $"SistemTanim with ID {id} not found." });
            }

            return Ok(SistemTanim);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SistemTanimDto SistemTanimDto)
        {
            // Validate the incoming DTO
            var validationResult = await _SistemTanimValidator.ValidateAsync(SistemTanimDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var createdSistemTanim = await _SistemTanimServis.AddSistemTanimAsync(SistemTanimDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSistemTanim.TanimTipiId }, createdSistemTanim);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SistemTanimDto SistemTanimDto)
        {
            if (id != SistemTanimDto.TanimTipiId)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _SistemTanimValidator.ValidateAsync(SistemTanimDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var updatedSistemTanim = await _SistemTanimServis.UpdateSistemTanimAsync(id, SistemTanimDto);
            if (updatedSistemTanim == null)
            {
                return NotFound(new { Message = $"SistemTanim with ID {id} not found for update." });
            }

            return Ok(updatedSistemTanim);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _SistemTanimServis.DeleteSistemTanimAsync(id);
            if (!success)
            {
                return NotFound(new { Message = $"SistemTanim with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
