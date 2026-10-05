using APIGasoline.Application.Features.Privilegios.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class PrivilegioProfile : Profile
{
    public PrivilegioProfile()
    {
        CreateMap<Privilegio, PrivilegioDto>();
    }
}
