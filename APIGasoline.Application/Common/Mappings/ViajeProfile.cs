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
            .ForMember(d => d.FotoTableroFinalUrl, opt => opt.Ignore())
            // Las pausas son otra tabla: las agrega el handler con ConPausasAsync
            .ForCtorParam(nameof(ViajeDto.EstaPausado), opt => opt.MapFrom(_ => false))
            .ForCtorParam(nameof(ViajeDto.SegundosEnPausa), opt => opt.MapFrom(_ => 0L))
            .ForCtorParam(nameof(ViajeDto.Pausas), opt => opt.MapFrom(_ => (IReadOnlyList<ViajePausaDto>?)null))
            .ForMember(d => d.EstaPausado, opt => opt.Ignore())
            .ForMember(d => d.SegundosEnPausa, opt => opt.Ignore())
            .ForMember(d => d.Pausas, opt => opt.Ignore())
            // El operador viene de empleado: lo agrega el handler con ConOperadorAsync
            .ForCtorParam(nameof(ViajeDto.Operador), opt => opt.MapFrom(_ => (OperadorViajeDto?)null))
            .ForMember(d => d.Operador, opt => opt.Ignore());

        CreateMap<ViajePausa, ViajePausaDto>();
    }
}
