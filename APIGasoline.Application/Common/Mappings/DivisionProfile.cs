using APIGasoline.Application.Features.Divisiones.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class DivisionProfile : Profile
{
    public DivisionProfile()
    {
        CreateMap<Division, DivisionDto>();
    }
}
