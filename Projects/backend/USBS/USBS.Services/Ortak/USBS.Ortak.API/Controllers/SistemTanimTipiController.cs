using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using Ortak.API.ViewModels;
using Mapster;
using KonFwBaseModels;
using KonFw.ApiBase;


namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemTanimTipiController : BaseController<SistemTanimTipiDto, SistemTanimTipiDto, SistemTanimTipiDto>
    {
        private readonly ISistemTanimTipiServis _SistemTanimTipiServis;
        private readonly IValidator<SistemTanimTipiVM> _SistemTanimTipiValidator;

        public SistemTanimTipiController(
            ISistemTanimTipiServis sistemTanimTipiServis, 
            IValidator<SistemTanimTipiVM> sistemTanimTipiValidator)
            : base(sistemTanimTipiServis)
        {
            _SistemTanimTipiServis = sistemTanimTipiServis;
            _SistemTanimTipiValidator = sistemTanimTipiValidator;
        }
    }
}
