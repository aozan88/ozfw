using Microsoft.AspNetCore.Mvc;
using Ortak.Application.Business;
using USBS.Common.Dtos.OrtakDtos;
using FluentValidation;
using AutoMapper;
using Ortak.API.ViewModels;
using KonFwBaseModels;
using KonFw.ApiBase;

namespace Ortak.API.Controllers
{
  
    public class SistemTanimController : BaseController<SistemTanimDto, SistemTanimDto, SistemTanimDto>
    {
        private readonly ISistemTanimServis _SistemTanimServis;
        private readonly IValidator<SistemTanimVM> _SistemTanimValidator;
        private readonly IMapper _mapper;

        public SistemTanimController(
            ISistemTanimServis sistemTanimServis,
            IValidator<SistemTanimVM> sistemTanimValidator,
            IMapper mapper
        ) : base(sistemTanimServis) 
        {
            _SistemTanimServis = sistemTanimServis;
            _SistemTanimValidator = sistemTanimValidator;
            _mapper = mapper;
        }
        
    }
}
