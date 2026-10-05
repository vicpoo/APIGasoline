using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class UsuarioProfile : Profile
{
    public UsuarioProfile()
    {
        CreateMap<Usuario, UsuarioDto>();
    }
}