using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CarroProfile : Profile
{
    public CarroProfile()
    {
        // La URL no sale de la entidad: la genera el handler con IFileStorageService
        CreateMap<Carro, CarroDto>()
            .ForCtorParam(nameof(CarroDto.FotoVehiculoUrl), opt => opt.MapFrom(_ => (string?)null))
            .ForMember(d => d.FotoVehiculoUrl, opt => opt.Ignore());
    }
}
