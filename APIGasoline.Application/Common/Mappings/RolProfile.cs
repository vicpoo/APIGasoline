using APIGasoline.Application.Features.Roles.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class RolProfile : Profile
{
    public RolProfile()
    {
        CreateMap<Rol, RolDto>();
    }
}
