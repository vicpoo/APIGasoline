using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Features.Usuarios.DTOs;
using APIGasoline.Domain.Interfaces;
using AutoMapper;
using MediatR;

namespace APIGasoline.Application.Features.Usuarios.Queries;

public record GetUsuarioByIdQuery(int UsuarioId) : IRequest<UsuarioDto>;

public class GetUsuarioByIdQueryHandler : IRequestHandler<GetUsuarioByIdQuery, UsuarioDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUsuarioByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<UsuarioDto> Handle(GetUsuarioByIdQuery request, CancellationToken cancellationToken)
    {
        var usuario = await _unitOfWork.Usuarios.GetByIdAsync(request.UsuarioId, cancellationToken)
            ?? throw new NotFoundException($"No existe un usuario con el id {request.UsuarioId}.");

        return _mapper.Map<UsuarioDto>(usuario);
    }
}