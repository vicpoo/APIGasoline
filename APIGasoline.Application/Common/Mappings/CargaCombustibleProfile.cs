using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CargaCombustibleProfile : Profile
{
    public CargaCombustibleProfile()
    {
        CreateMap<CargaCombustible, CargaCombustibleDto>();
    }
}
