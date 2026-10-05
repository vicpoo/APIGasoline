using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class ViajeProfile : Profile
{
    public ViajeProfile()
    {
        CreateMap<Viaje, ViajeDto>();
    }
}
