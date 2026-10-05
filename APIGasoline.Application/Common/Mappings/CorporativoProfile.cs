using APIGasoline.Application.Features.Corporativos.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CorporativoProfile : Profile
{
    public CorporativoProfile()
    {
        CreateMap<Corporativo, CorporativoDto>();
    }
}
