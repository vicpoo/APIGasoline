using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class SolicitudReservaProfile : Profile
{
    public SolicitudReservaProfile()
    {
        CreateMap<SolicitudReserva, SolicitudReservaDto>();
    }
}
