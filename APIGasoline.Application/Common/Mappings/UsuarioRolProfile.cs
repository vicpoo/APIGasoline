using APIGasoline.Application.Features.UsuarioRoles.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class UsuarioRolProfile : Profile
{
    public UsuarioRolProfile()
    {
        CreateMap<UsuarioRol, UsuarioRolDto>();
    }
}
