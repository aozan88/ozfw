using Microsoft.AspNetCore.Mvc;
using Sistem.Application.Business;
using Sistem.Application.DtoModels;
using FluentValidation;
using USBS.Common.Dtos.OrtakDtos;
using KonFw.ApiBase;
using Sistem.API.ViewModels;

namespace Sistem.API.Controllers
{
    
    public class SistemParametreController : BaseController<SistemParametreDto, SistemParametreDto, SistemParametreDto>
    {
        private readonly ISistemParametreServis _SistemParametreService;
        private readonly IValidator<SistemParametreVM> _SistemParametreValidator;

        public SistemParametreController(
            ISistemParametreServis SistemParametreService, 
            IValidator<SistemParametreVM> SistemParametreValidator
            ) : base(SistemParametreService)
        {
            _SistemParametreService = SistemParametreService;
            _SistemParametreValidator = SistemParametreValidator;

        }
    }
}
