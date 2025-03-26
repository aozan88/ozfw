using Microsoft.AspNetCore.Mvc;
using Sistem.Application.Business;
using Sistem.Application.DtoModels;
using FluentValidation;

namespace Sistem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemConfigController : ControllerBase
    {
        private readonly SistemConfigBusinessService _SistemConfigService;
        private readonly IValidator<SistemConfigDto> _SistemConfigValidator;

        public SistemConfigController(SistemConfigBusinessService SistemConfigService, IValidator<SistemConfigDto> SistemConfigValidator)
        {
            _SistemConfigService = SistemConfigService;
            _SistemConfigValidator = SistemConfigValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var SistemConfigs = await _SistemConfigService.GetAllActiveSistemConfigsAsync();
            return Ok(SistemConfigs);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var SistemConfig = await _SistemConfigService.GetByIdAsync(id);
            if (SistemConfig == null)
            {
                return NotFound(new { Message = $"SistemConfig with ID {id} not found." });
            }

            return Ok(SistemConfig);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SistemConfigDto SistemConfigDto)
        {
            // Validate the incoming DTO
            var validationResult = await _SistemConfigValidator.ValidateAsync(SistemConfigDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var createdSistemConfig = await _SistemConfigService.AddSistemConfigAsync(SistemConfigDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSistemConfig.SistemConfigTipiId }, createdSistemConfig);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SistemConfigDto SistemConfigDto)
        {
            if (id != SistemConfigDto.SistemConfigTipiId)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _SistemConfigValidator.ValidateAsync(SistemConfigDto);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            var updatedSistemConfig = await _SistemConfigService.UpdateSistemConfigAsync(id, SistemConfigDto);
            if (updatedSistemConfig == null)
            {
                return NotFound(new { Message = $"SistemConfig with ID {id} not found for update." });
            }

            return Ok(updatedSistemConfig);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _SistemConfigService.DeleteSistemConfigAsync(id);
            if (!success)
            {
                return NotFound(new { Message = $"SistemConfig with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
