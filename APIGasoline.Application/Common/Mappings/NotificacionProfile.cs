using APIGasoline.Application.Features.Notificaciones.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class NotificacionProfile : Profile
{
    public NotificacionProfile()
    {
        CreateMap<Notificacion, NotificacionDto>();
    }
}
