using APIGasoline.Application.Features.SolicitudesReserva.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class SolicitudReservaProfile : Profile
{
    public SolicitudReservaProfile()
    {
        // Solicitante y Carro vienen de otras tablas: los agrega el handler con DatosSolicitud.ConDatosAsync
        CreateMap<SolicitudReserva, SolicitudReservaDto>()
            .ForCtorParam(nameof(SolicitudReservaDto.Solicitante), opt => opt.MapFrom(_ => (EmpleadoResumenDto?)null))
            .ForCtorParam(nameof(SolicitudReservaDto.Carro), opt => opt.MapFrom(_ => (CarroResumenDto?)null))
            .ForMember(d => d.Solicitante, opt => opt.Ignore())
            .ForMember(d => d.Carro, opt => opt.Ignore());
    }
}
