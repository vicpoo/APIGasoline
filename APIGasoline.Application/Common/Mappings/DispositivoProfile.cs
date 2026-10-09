using APIGasoline.Application.Features.Dispositivos.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class DispositivoProfile : Profile
{
    public DispositivoProfile()
    {
        CreateMap<DispositivoEmpleado, DispositivoDto>();
    }
}
