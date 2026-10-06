using APIGasoline.Application.Features.Empleados.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class EmpleadoProfile : Profile
{
    public EmpleadoProfile()
    {
        // Las URLs no salen de la entidad: las genera el handler con IFileStorageService
        CreateMap<Empleado, EmpleadoDto>()
            .ForCtorParam(nameof(EmpleadoDto.FotoPerfilUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForCtorParam(nameof(EmpleadoDto.FotoLicenciaUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForMember(d => d.FotoPerfilUrl, opt => opt.Ignore())
            .ForMember(d => d.FotoLicenciaUrl, opt => opt.Ignore());
    }
}
