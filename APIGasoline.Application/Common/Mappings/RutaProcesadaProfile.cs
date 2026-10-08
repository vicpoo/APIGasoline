using APIGasoline.Application.Features.RutasProcesadas.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class RutaProcesadaProfile : Profile
{
    public RutaProcesadaProfile()
    {
        CreateMap<RutaProcesada, RutaProcesadaDto>();
    }
}
