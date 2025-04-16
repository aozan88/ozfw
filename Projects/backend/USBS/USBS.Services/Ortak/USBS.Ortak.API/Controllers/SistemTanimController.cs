using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using Mapster;
using Ortak.API.ViewModels;
using KonFwBaseModels;
using KonFw.ApiBase;

namespace Ortak.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemTanimController : BaseController<SistemTanimDto, SistemTanimDto, SistemTanimDto>
    {
        private readonly ISistemTanimServis _SistemTanimServis;
        private readonly IValidator<SistemTanimVM> _SistemTanimValidator;

        public SistemTanimController(
            ISistemTanimServis sistemTanimServis,
            IValidator<SistemTanimVM> sistemTanimValidator
        ) : base(sistemTanimServis) 
        {
            _SistemTanimServis = sistemTanimServis;
            _SistemTanimValidator = sistemTanimValidator;
        }
        
    }
}
