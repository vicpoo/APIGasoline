using APIGasoline.Application.Features.Carros.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class CarroProfile : Profile
{
    public CarroProfile()
    {
        CreateMap<Carro, CarroDto>();
    }
}
