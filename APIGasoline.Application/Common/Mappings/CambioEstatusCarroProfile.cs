using APIGasoline.Application.Features.CambiosEstatusCarro.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CambioEstatusCarroProfile : Profile
{
    public CambioEstatusCarroProfile()
    {
        // La URL no sale de la entidad: la genera el handler con IFileStorageService
        CreateMap<CambioEstatusCarro, CambioEstatusCarroDto>()
            .ForCtorParam(nameof(CambioEstatusCarroDto.FotoEvidenciaUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForMember(d => d.FotoEvidenciaUrl, opt => opt.Ignore());
    }
}
