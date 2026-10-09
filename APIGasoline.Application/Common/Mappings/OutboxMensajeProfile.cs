using APIGasoline.Application.Features.Outbox.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class OutboxMensajeProfile : Profile
{
    public OutboxMensajeProfile()
    {
        CreateMap<OutboxMensaje, OutboxMensajeDto>();
    }
}
