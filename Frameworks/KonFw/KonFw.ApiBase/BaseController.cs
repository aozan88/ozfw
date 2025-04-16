using KonFw.ServiceBase;
using KonFwBaseModels;
using Microsoft.AspNetCore.Mvc;


namespace KonFw.ApiBase
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class BaseController<TDto, TRequestDto, TResponseDto> : ControllerBase,
        IBaseControllerInterface<TDto, TRequestDto, TResponseDto>
        where TDto : BaseDto
    {
        
        protected readonly IBaseServiceInterface<TDto, TRequestDto, TResponseDto> _service;

        public BaseController(IBaseServiceInterface<TDto, TRequestDto, TResponseDto> service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        // Ortak yanıt oluşturma metodu
        public async Task<ActionResult<T>> CreateResponseAsync<T>(T responseObject, int status)
        {
            return await Task.FromResult(new ObjectResult(responseObject) { StatusCode = status });
        }

        // Get Operations
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TDto>> GetByIdAsync(Guid id)
        {
            try
            {
                var result = await _service.GetByIdAsync(id);
                if (result == null)
                    return NotFound($"Entity with ID {id} not found.");

                return Ok(result);
            }
            catch (Exception ex)
            {
                var errorResponse = Activator.CreateInstance<TDto>();
                if (errorResponse is BaseDto baseDto)
                {
                   
                }
                return await CreateResponseAsync(errorResponse, 500);
            }
        }

        [HttpGet("byFilter")]
        public  async Task<ActionResult<BaseResponseDto<TResponseDto>>> GetByFilterAsync(
            BaseRequestDto<TRequestDto> baseRequestDto)
        {
            try
            {
                var result = await _service.GetByFilterAsync(baseRequestDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new BaseResponseDto<TResponseDto>
                {
                    IsSuccess = false,
                    Mesaj = ex.Message
                }, 500);
            }
        }

        [HttpGet]
        public  async Task<ActionResult<List<TDto>>> GetAllAsync()
        {
            try
            {
                var result = await _service.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new List<TDto>
                {
                    
                    
                }, 500);
            }
        }

        // Create Operations
        [HttpPost("Create")]
        public virtual async Task<ActionResult<BaseResponseDto<TResponseDto>>> CreateAsync(
            BaseRequestDto<TRequestDto> baseRequestDto)
        {
            try
            {
                var result = await _service.CreateAsync(baseRequestDto);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new BaseResponseDto<TResponseDto>
                {
                    IsSuccess = false,
                    Mesaj = ex.Message
                }, 500);
            }
        }

        [HttpPost("manyCreate")]
        public  async Task<ActionResult<HashSet<TDto>>> CreateManyAsync(HashSet<TDto> dtoList)
        {
            try
            {
                var result = await _service.CreateManyAsync(dtoList);
                return StatusCode(201, result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new HashSet<TDto>
                {


                }, 500);
            }
        }

        // Update Operations
        [HttpPut("update")]
        public  async Task<ActionResult<BaseResponseDto<TResponseDto>>> UpdateAsync(
         [FromBody] BaseRequestDto<TRequestDto> baseRequestDto)
        {
            try
            {
                var result = await _service.UpdateAsync(baseRequestDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new BaseResponseDto<TResponseDto>
                {
                    IsSuccess = false,
                    Mesaj = ex.Message
                }, 500);
            }
        }

        [HttpPut("manyUpdate")]
        public  async Task<ActionResult<HashSet<TDto>>> UpdateManyAsync(HashSet<TDto> dtoList)
        {
            try
            {
                var result = await _service.UpdateManyAsync(dtoList);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new HashSet<TDto>
                {


                }, 500);
            }
        }

        // Delete Operations
        [HttpDelete("{id:guid}")]
        public  async Task<ActionResult> DeleteAsync(Guid id)
        {
            await _service.DeleteAsync(id);

            return NoContent();

        }

        [HttpPost("delete")]
        public  async Task<ActionResult<BaseResponseDto<TResponseDto>>> DeleteByFilterAsync(
            BaseRequestDto<TRequestDto> baseRequestDto)
        {
            try
            {
                var result = await _service.DeleteByFilterAsync(baseRequestDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return await CreateResponseAsync(new BaseResponseDto<TResponseDto>
                {
                    IsSuccess = false,
                    Mesaj = ex.Message
                }, 500);
            }
        }
    }

}