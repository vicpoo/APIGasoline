using APIGasoline.Application.Features.Auditoria.DTOs;
using APIGasoline.Domain.Entities;
using AutoMapper;

namespace APIGasoline.Application.Common.Mappings;

public class RevisionAuditoriaProfile : Profile
{
    public RevisionAuditoriaProfile()
    {
        CreateMap<RevisionAuditoria, RevisionAuditoriaDto>();
    }
}
