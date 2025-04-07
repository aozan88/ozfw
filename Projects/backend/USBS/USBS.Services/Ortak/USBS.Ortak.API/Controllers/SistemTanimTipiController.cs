using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using Ortak.API.ViewModels;
using AutoMapper;

namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemTanimTipiController : ControllerBase
    {
        private readonly ISistemTanimTipiServis _SistemTanimTipiServis;
        private readonly IValidator<SistemTanimTipiVM> _SistemTanimTipiValidator;
        private readonly IMapper _mapper;

        public SistemTanimTipiController(ISistemTanimTipiServis sistemTanimTipiServis, IValidator<SistemTanimTipiVM> sistemTanimTipiValidator, IMapper mapper)
        {
            _SistemTanimTipiServis = sistemTanimTipiServis;
            _SistemTanimTipiValidator = sistemTanimTipiValidator;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var SistemTanimTipis = await _SistemTanimTipiServis.GetAllActiveSistemTanimTipisAsync();
            return Ok(SistemTanimTipis);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var SistemTanimTipi = await _SistemTanimTipiServis.GetByIdAsync(id);
            if (SistemTanimTipi == null)
            {
                return NotFound(new { Message = $"SistemTanimTipi with ID {id} not found." });
            }

            return Ok(SistemTanimTipi);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SistemTanimTipiVM sistemTanimTipiVm)
        {
            // Validate the incoming DTO
            var validationResult = await _SistemTanimTipiValidator.ValidateAsync(sistemTanimTipiVm);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            SistemTanimTipiDto sistemTanimTipiDto = new SistemTanimTipiDto();
            _mapper.Map(sistemTanimTipiVm, sistemTanimTipiDto);
            var createdSistemTanimTipi = await _SistemTanimTipiServis.AddSistemTanimTipiAsync(sistemTanimTipiDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSistemTanimTipi.Id }, createdSistemTanimTipi);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SistemTanimTipiVM sistemTanimTipiVm)
        {
            if (id != sistemTanimTipiVm.Id)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _SistemTanimTipiValidator.ValidateAsync(sistemTanimTipiVm);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            SistemTanimTipiDto sistemTanimTipiDto = new SistemTanimTipiDto();
            _mapper.Map(sistemTanimTipiVm, sistemTanimTipiDto);
            var updatedSistemTanimTipi = await _SistemTanimTipiServis.UpdateSistemTanimTipiAsync(id, sistemTanimTipiDto);
            if (updatedSistemTanimTipi == null)
            {
                return NotFound(new { Message = $"SistemTanimTipi with ID {id} not found for update." });
            }

            return Ok(updatedSistemTanimTipi);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _SistemTanimTipiServis.DeleteSistemTanimTipiAsync(id);
            if (!success)
            {
                return NotFound(new { Message = $"SistemTanimTipi with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
