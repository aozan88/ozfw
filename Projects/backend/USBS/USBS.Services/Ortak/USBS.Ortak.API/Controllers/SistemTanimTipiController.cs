using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using Ortak.API.ViewModels;
using AutoMapper;
using KonFwBaseModels;
using KonFw.ApiBase;


namespace Ortak.API.Controllers
{
    public class SistemTanimTipiController : BaseController<SistemTanimTipiDto, SistemTanimTipiDto, SistemTanimTipiDto>
    {
        private readonly ISistemTanimTipiServis _SistemTanimTipiServis;
        private readonly IValidator<SistemTanimTipiVM> _SistemTanimTipiValidator;
        private readonly IMapper _mapper;

        public SistemTanimTipiController(
            ISistemTanimTipiServis sistemTanimTipiServis, 
            IValidator<SistemTanimTipiVM> sistemTanimTipiValidator, 
            IMapper mapper) : base(sistemTanimTipiServis)
        {
            _SistemTanimTipiServis = sistemTanimTipiServis;
            _SistemTanimTipiValidator = sistemTanimTipiValidator;
            _mapper = mapper;
        }
    }
}
