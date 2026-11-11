using System;
using System.Collections.Generic;
using System.Text;

using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using APIGasoline.Application.Common.Messaging;

namespace APIGasoline.Application.Features.Usuarios.Queries;

public sealed record GetUsuariosQuery : IQuery<IReadOnlyList<UsuarioDto>>;

public sealed class GetUsuariosQueryHandler : IQueryHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsuariosQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<UsuarioDto>> Handle(GetUsuariosQuery request, CancellationToken cancellationToken)
    {
        var usuarios = await _unitOfWork.Usuarios.GetAllAsync(cancellationToken);
        return _mapper.Map<IReadOnlyList<UsuarioDto>>(usuarios);
    }
}
