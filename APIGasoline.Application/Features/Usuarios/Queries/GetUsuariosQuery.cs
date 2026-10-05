using System;
using System.Collections.Generic;
using System.Text;

using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Queries;

public record GetUsuariosQuery : IRequest<IReadOnlyList<UsuarioDto>>;

public class GetUsuariosQueryHandler : IRequestHandler<GetUsuariosQuery, IReadOnlyList<UsuarioDto>>
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
