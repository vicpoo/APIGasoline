using APIGasoline.Application.Features.Viajes.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class ViajeProfile : Profile
{
    public ViajeProfile()
    {
        // Las URLs no salen de la entidad: las genera el handler con IFileStorageService
        CreateMap<Viaje, ViajeDto>()
            .ForCtorParam(nameof(ViajeDto.FotoTableroInicialUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForCtorParam(nameof(ViajeDto.FotoTableroFinalUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForMember(d => d.FotoTableroInicialUrl, opt => opt.Ignore())
            .ForMember(d => d.FotoTableroFinalUrl, opt => opt.Ignore());
    }
}
