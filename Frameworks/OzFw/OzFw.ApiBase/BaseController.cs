using FluentValidation;
using OzFw.ServiceBase;
using OzFwBaseHelpers;
using OzFwBaseModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace OzFw.ApiBase
{
    /// <summary>
    /// Standart CRUD uçları (hepsi POST): getById, getAll, create, manyCreate, update, manyUpdate, deleteByFilter, delete.
    /// İstekler RuleSet'lerle doğrulanır (bkz. BaseRequestDtoValidation, BaseGetByIdDtoValidation).
    /// DI'da IValidator&lt;TRequestDto&gt; kayıtlıysa Payload onunla da doğrulanır.
    /// Doğrulama hatası: 400, beklenmeyen hata: 500; gövde her zaman BaseResponseDto.
    /// </summary>
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
        [HttpPost("getById")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> GetByIdAsync([FromBody] BaseGetByIdDto baseGetByIdDto) =>
            ExecuteAsync(
                () => ValidateGetByIdAsync(baseGetByIdDto),
                () => _service.GetByIdAsync(baseGetByIdDto.Payload));

        [HttpPost("getAll")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> GetAllAsync([FromBody] BaseRequestDto<TRequestDto> baseRequestDto) =>
            ExecuteAsync(
                () => ValidateRequestAsync(baseRequestDto, BaseRequestDtoValidation<TRequestDto>.Common, BaseRequestDtoValidation<TRequestDto>.GetAll),
                () => _service.GetAllAsync(baseRequestDto));

        // Create Operations
        [HttpPost("create")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> CreateAsync([FromBody] BaseRequestDto<TRequestDto> baseRequestDto) =>
            ExecuteAsync(
                () => ValidateRequestAsync(baseRequestDto, BaseRequestDtoValidation<TRequestDto>.Common, BaseRequestDtoValidation<TRequestDto>.CreateOrUpdate),
                () => _service.CreateAsync(baseRequestDto),
                successStatus: 201);

        [HttpPost("manyCreate")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> CreateManyAsync([FromBody] List<TDto> dtoList) =>
            ExecuteAsync(
                () => ValidateListAsync(dtoList),
                () => _service.CreateManyAsync(dtoList),
                successStatus: 201);

        // Update Operations
        [HttpPost("update")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> UpdateAsync([FromBody] BaseRequestDto<TRequestDto> baseRequestDto) =>
            ExecuteAsync(
                () => ValidateRequestAsync(baseRequestDto, BaseRequestDtoValidation<TRequestDto>.Common, BaseRequestDtoValidation<TRequestDto>.CreateOrUpdate),
                () => _service.UpdateAsync(baseRequestDto));

        [HttpPost("manyUpdate")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> UpdateManyAsync([FromBody] List<TDto> dtoList) =>
            ExecuteAsync(
                () => ValidateListAsync(dtoList),
                () => _service.UpdateManyAsync(dtoList));

        // Delete Operations
        [HttpPost("deleteByFilter")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> DeleteByFilterAsync([FromBody] BaseRequestDto<TRequestDto> baseRequestDto) =>
            ExecuteAsync(
                () => ValidateRequestAsync(baseRequestDto, BaseRequestDtoValidation<TRequestDto>.Common),
                () => _service.DeleteByFilterAsync(baseRequestDto));

        [HttpPost("delete")]
        public virtual Task<ActionResult<BaseResponseDto<TResponseDto>>> DeleteAsync([FromBody] BaseGetByIdDto baseGetByIdDto) =>
            ExecuteAsync(
                () => ValidateGetByIdAsync(baseGetByIdDto),
                () => _service.DeleteAsync(baseGetByIdDto.Payload));

        #region Yardımcılar

        protected async Task<ActionResult<BaseResponseDto<TResponseDto>>> ExecuteAsync(
            Func<Task<List<string>>> validate,
            Func<Task<BaseResponseDto<TResponseDto>>> action,
            int successStatus = 200)
        {
            var errors = await validate();
            if (errors.Count > 0)
                return Error(400, "Geçersiz istek parametresi.", errors);

            try
            {
                var result = await action();
                return StatusCode(result.IsSuccess ? successStatus : 200, result);
            }
            catch (ArgumentException ex) // QueryFieldException dahil: istemci kaynaklı
            {
                return Error(400, ex.Message);
            }
            catch (Exception ex)
            {
                return Error(500, ex.Message);
            }
        }

        protected ActionResult<BaseResponseDto<TResponseDto>> Error(int status, string message, List<string>? fieldErrors = null) =>
            StatusCode(status, new BaseResponseDto<TResponseDto>
            {
                IsSuccess = false,
                Mesaj = message,
                FieldNames = fieldErrors ?? new List<string>()
            });

        protected async Task<List<string>> ValidateRequestAsync(BaseRequestDto<TRequestDto>? request, params string[] ruleSets)
        {
            if (request == null)
                return new List<string> { "İstek gövdesi boş olamaz." };

            var services = HttpContext.RequestServices;
            var validator = services.GetService<IValidator<BaseRequestDto<TRequestDto>>>()
                            ?? new BaseRequestDtoValidation<TRequestDto>(services.GetService<IValidator<TRequestDto>>());

            // IncludeRulesNotInRuleSet: Payload validator'ının RuleSet'siz kuralları da çalışsın
            var result = await validator.ValidateAsync(request, o => o.IncludeRuleSets(ruleSets).IncludeRulesNotInRuleSet());
            return ToErrors(result);
        }

        protected async Task<List<string>> ValidateGetByIdAsync(BaseGetByIdDto? request)
        {
            if (request == null)
                return new List<string> { "İstek gövdesi boş olamaz." };

            var validator = HttpContext.RequestServices.GetService<IValidator<BaseGetByIdDto>>() ?? new BaseGetByIdDtoValidation();
            var result = await validator.ValidateAsync(request, o => o.IncludeRuleSets(BaseGetByIdDtoValidation.Common, BaseGetByIdDtoValidation.GetById));
            return ToErrors(result);
        }

        protected async Task<List<string>> ValidateListAsync(List<TDto>? dtoList)
        {
            if (dtoList == null || dtoList.Count == 0)
                return new List<string> { "Liste boş olamaz." };

            var validator = HttpContext.RequestServices.GetService<IValidator<TDto>>();
            if (validator == null)
                return new List<string>();

            var errors = new List<string>();
            for (var i = 0; i < dtoList.Count; i++)
            {
                var result = await validator.ValidateAsync(dtoList[i]);
                errors.AddRange(result.Errors.Select(e => $"[{i}].{e.PropertyName}: {e.ErrorMessage}"));
            }
            return errors;
        }

        private static List<string> ToErrors(FluentValidation.Results.ValidationResult result) =>
            result.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();

        #endregion
    }
}
