using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using Ortak.API.ViewModels;
using AutoMapper;
using KonFwBaseModels;


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
            var result = await _SistemTanimTipiServis.GetAllActiveSistemTanimTipisAsync();

            if (!result.IsSuccess || result.ObjectValue == null)
            {
                return NotFound(new { Message = "Aktif sistem tanýmlarý bulunamadý." });
            }

            return Ok(result);
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
        public async Task<IActionResult> Create([FromBody] BaseRequestDto<SistemTanimTipiVM> request)
        {
            if (request?.SearchValue == null)
            {
                return BadRequest("SearchValue is required.");
            }

            var validationResult = await _SistemTanimTipiValidator.ValidateAsync(request.SearchValue);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            var dto = _mapper.Map<SistemTanimTipiDto>(request.SearchValue);
            var result = await _SistemTanimTipiServis.AddSistemTanimTipiAsync(dto);

            if (!result.IsSuccess || result.ObjectValue == null || !result.ObjectValue.Any())
            {
                return StatusCode(500, "Kayýt oluþturulamadý."); // Daha net bir hata mesajý
            }

            var createdId = result.ObjectValue.First().Id;
            return CreatedAtAction(nameof(GetById), new { id = createdId }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromBody] BaseRequestDto<SistemTanimTipiVM> request)
        {
            if (request?.SearchValue == null)
            {
                return BadRequest("SearchValue is required.");
            }

            // Validate the DTO
            var validationResult = await _SistemTanimTipiValidator.ValidateAsync(request.SearchValue);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
                return BadRequest(errors);
            }

            var dto = _mapper.Map<SistemTanimTipiDto>(request.SearchValue);

            var result = await _SistemTanimTipiServis.UpdateSistemTanimTipiAsync(dto);

            if (!result.IsSuccess || result.ObjectValue == null)
            {
                return NotFound(new { Message = $"SistemTanimTipi with ID  not found or update failed." });
            }

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _SistemTanimTipiServis.DeleteSistemTanimTipiAsync(id);
            if (!result.IsSuccess)
            {
                return NotFound(new { Message = $"SistemTanimTipi with ID {id} not found for deletion." });
            }

            return NoContent();
        }
    }
}
