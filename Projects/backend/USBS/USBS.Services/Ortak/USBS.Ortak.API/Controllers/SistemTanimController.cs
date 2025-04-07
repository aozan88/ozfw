using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using AutoMapper;
using Ortak.API.ViewModels;

namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemTanimController : ControllerBase
    {
        private readonly ISistemTanimServis _SistemTanimServis;
        private readonly IValidator<SistemTanimVM> _SistemTanimValidator;
        private readonly IMapper _mapper;

        public SistemTanimController(ISistemTanimServis sistemTanimServis, IValidator<SistemTanimVM> sistemTanimValidator, IMapper mapper)
        {
            _SistemTanimServis = sistemTanimServis;
            _SistemTanimValidator = sistemTanimValidator;
            _mapper = mapper;
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
        public async Task<IActionResult> Create([FromBody] SistemTanimVM sistemTanimVm)
        {
            // Validate the incoming DTO
            var validationResult = await _SistemTanimValidator.ValidateAsync(sistemTanimVm);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }
            SistemTanimDto sistemTanimDto = new SistemTanimDto();
            _mapper.Map(sistemTanimVm, sistemTanimDto);
            var createdSistemTanim = await _SistemTanimServis.AddSistemTanimAsync(sistemTanimDto);
            return CreatedAtAction(nameof(GetById), new { id = createdSistemTanim.TanimTipiId }, createdSistemTanim);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] SistemTanimVM sistemTanimVm)
        {
            if (id != sistemTanimVm.TanimTipiId)
            {
                return BadRequest(new { Message = "ID mismatch between the route and the payload." });
            }

            // Validate the DTO
            var validationResult = await _SistemTanimValidator.ValidateAsync(sistemTanimVm);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            SistemTanimDto sistemTanimDto = new SistemTanimDto();
            _mapper.Map(sistemTanimVm, sistemTanimDto);
            var updatedSistemTanim = await _SistemTanimServis.UpdateSistemTanimAsync(id, sistemTanimDto);
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
