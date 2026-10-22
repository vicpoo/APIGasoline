using APIGasoline.Application.Common.Security;
using APIGasoline.Application.Common.Exceptions;
using APIGasoline.Application.Common.Interfaces;
using APIGasoline.Application.Features.Empleados.Common;
using APIGasoline.Domain.Interfaces;
using MediatR;

namespace APIGasoline.Application.Features.Empleados.Commands;

/// <summary>
/// Quita la foto de perfil o la licencia: limpia la llave en BD y después elimina el archivo.
/// </summary>
public record EliminarFotoEmpleadoCommand(Guid EmpleadoId, TipoFotoEmpleado Tipo) : IRequest, IRecursoPropio
{
    /// <summary>Sus propias fotos, o las de cualquiera con empleado.gestionar.</summary>
    public bool EsPropio(IUsuarioActual usuario)
        => EmpleadoId == usuario.EmpleadoId;

    public string PrivilegioParaOtros => Permisos.EmpleadoGestionar;
}

public class EliminarFotoEmpleadoCommandHandler : IRequestHandler<EliminarFotoEmpleadoCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IFileStorageService _storage;

    public EliminarFotoEmpleadoCommandHandler(IUnitOfWork unitOfWork, IFileStorageService storage)
    {
        _unitOfWork = unitOfWork;
        _storage = storage;
    }

    public async Task Handle(EliminarFotoEmpleadoCommand request, CancellationToken cancellationToken)
    {
        var empleado = await _unitOfWork.Empleados.GetByIdAsync(request.EmpleadoId, cancellationToken)
            ?? throw new NotFoundException($"No existe un empleado con el id {request.EmpleadoId}.");

        var archivo = request.Tipo == TipoFotoEmpleado.Licencia ? empleado.FotoLicencia : empleado.FotoPerfil;

        if (archivo is null)
            throw new NotFoundException(request.Tipo == TipoFotoEmpleado.Licencia
                ? "El empleado no tiene licencia registrada."
                : "El empleado no tiene foto de perfil.");

        if (request.Tipo == TipoFotoEmpleado.Licencia)
            empleado.CambiarFotoLicencia(null);
        else
            empleado.CambiarFotoPerfil(null);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _storage.IntentarEliminarAsync(archivo);
    }
}
