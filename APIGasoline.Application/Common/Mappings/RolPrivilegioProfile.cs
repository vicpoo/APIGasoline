using APIGasoline.Application.Features.RolPrivilegios.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class RolPrivilegioProfile : Profile
{
    public RolPrivilegioProfile()
    {
        CreateMap<RolPrivilegio, RolPrivilegioDto>();
    }
}
