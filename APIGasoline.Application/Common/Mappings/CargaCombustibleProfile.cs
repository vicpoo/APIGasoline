using APIGasoline.Application.Features.CargasCombustible.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CargaCombustibleProfile : Profile
{
    public CargaCombustibleProfile()
    {
        // La URL no sale de la entidad: la genera el handler con IFileStorageService
        CreateMap<CargaCombustible, CargaCombustibleDto>()
            .ForCtorParam(nameof(CargaCombustibleDto.FotoComprobanteUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForMember(d => d.FotoComprobanteUrl, opt => opt.Ignore());
    }
}
