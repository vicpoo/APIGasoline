using APIGasoline.Application.Features.PuntosRuta.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class PuntoRutaProfile : Profile
{
    public PuntoRutaProfile()
    {
        CreateMap<PuntoRuta, PuntoRutaDto>();
    }
}
