using APIGasoline.Application.Features.CentrosCosto.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CentroCostoProfile : Profile
{
    public CentroCostoProfile()
    {
        CreateMap<CentroCosto, CentroCostoDto>();
    }
}
