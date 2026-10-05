using System;
using System.Collections.Generic;
using System.Text;
using APIGasoline.Domain.Enums;

namespace APIGasoline.Application.Features.Usuarios.DTOs;

public record UsuarioDto(int UsuarioId, string Email, EstadoUsuario Estado);