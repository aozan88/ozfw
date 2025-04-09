using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using AutoMapper;
using Ortak.API.ViewModels;
using KonFwBaseModels;
using Swashbuckle.AspNetCore.Filters;

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
            var result = await _SistemTanimServis.GetAllActiveSistemTanimsAsync();

            if (!result.IsSuccess || result.ObjectValue == null)
            {
                return NotFound(new { Message = "Aktif sistem tanýmlarý bulunamadý." });
            }

            return Ok(result);
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
        public async Task<IActionResult> Create([FromBody] BaseRequestDto<SistemTanimVM>? request)
        {

            if (request?.SearchValue == null)
            {
                return BadRequest("SearchValue is required.");
            }

            var validationResult = await _SistemTanimValidator.ValidateAsync(request.SearchValue);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            var dto = _mapper.Map<SistemTanimDto>(request.SearchValue);
            var result = await _SistemTanimServis.AddSistemTanimAsync(dto);

            if (!result.IsSuccess || result.ObjectValue == null || !result.ObjectValue.Any())
            {
                return StatusCode(500, "Kayýt oluþturulamadý."); 
            }

            var createdId = result.ObjectValue.First().Id;
            return CreatedAtAction(nameof(GetById), new { id = createdId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromBody] BaseRequestDto<SistemTanimVM> sistemTanimVm)
        {

            if (sistemTanimVm?.SearchValue == null)
            {
                return BadRequest(sistemTanimVm);
            }
            
            // Validate the DTO
            var validationResult = await _SistemTanimValidator.ValidateAsync(sistemTanimVm.SearchValue);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors.Select(e => e.ErrorMessage));
            }

            SistemTanimDto sistemTanimDto = new SistemTanimDto();
            _mapper.Map(sistemTanimVm.SearchValue, sistemTanimDto);
            var updatedSistemTanim = await _SistemTanimServis.UpdateSistemTanimAsync(sistemTanimDto);

            if (updatedSistemTanim == null)
            {
                return NotFound(new { Message = $"SistemTanim with ID {sistemTanimDto.Id} not found for update." });
            }

            return Ok(updatedSistemTanim);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _SistemTanimServis.DeleteSistemTanimAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Message = $"SistemTanim with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
