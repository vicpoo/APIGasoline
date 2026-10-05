using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CambioEstatusCarroProfile : Profile
{
    public CambioEstatusCarroProfile()
    {
        CreateMap<CambioEstatusCarro, CambioEstatusCarroDto>();
    }
}
